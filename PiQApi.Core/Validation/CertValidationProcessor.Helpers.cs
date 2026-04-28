// PiQApi.Core/Validation/CertValidationProcessor.Helpers.cs
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Abstractions.Context;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using PiQApi.Abstractions.Core;

namespace PiQApi.Core.Validation;

public partial class CertValidationProcessor
{
    // Cache of compiled generic methods for better performance
    private readonly ConcurrentDictionary<Type, Func<object, CertValidationContext, Type, string, ICertValidationResult>> _validateFuncCache = new();
    private readonly ConcurrentDictionary<Type, Func<object, CertValidationContext, CancellationToken, Task<ICertValidationResult>>> _validateAsyncFuncCache = new();

    /// <summary>
    /// Type-specific validation for any entity type
    /// </summary>
    private ICertValidationResult ValidateTypedEntity<T>(T entity, CertValidationContext context, Type entityType, string entityTypeName) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        // Convert correlation ID to string consistently
        string correlationId = context.CorrelationId;

        var builder = CreateResultBuilder(correlationId);

        // Get typed rules for this entity type with thread-safety
        List<ICertValidationRule<T>> typedRules = GetTypedRulesThreadSafe<T>(entityType);

        if (typedRules.Count == 0)
        {
            LogNoRulesForEntity(_logger, entityTypeName, null);
            return CreateSuccessResult(correlationId);
        }

        LogFoundRules(_logger, entityTypeName, typedRules.Count, null);

        var isStrict = context.ValidationMode == PiQApi.Abstractions.Enums.ValidationModeType.Strict;

        // Look for resource scope in context for tracking validation
        ICertResourceScope? resourceScope = null;
        if (context.TryGetProperty("ResourceScope", out ICertResourceScope scope))
        {
            resourceScope = scope;
        }

        // Create a validation operation resource ID for tracking
        var validationOperationId = Guid.NewGuid().ToString("N");
        string operationMetricKey = $"Validation_{entityTypeName}_{validationOperationId}";

        // Track start time
        var startTime = context.StartTimer(operationMetricKey);

        foreach (var rule in typedRules)
        {
            try
            {
                // Create properties dictionary with rule information
                var properties = new Dictionary<string, object>
                {
                    ["RuleId"] = rule.RuleId,
                    ["RuleType"] = rule.GetType().Name,
                    ["EntityType"] = entityTypeName
                };

                // Create a resource object for this rule execution
                var ruleResource = new CertValidationRuleResource(
                    $"{rule.RuleId}_{Guid.NewGuid():N}",
                    "ValidationRule",
                    rule.RuleId,
                    properties: properties);

                // Add resource tracking
                // Using ConfigureAwait(false).GetAwaiter().GetResult() for sync context
                resourceScope?.AddResourceAsync(ruleResource).ConfigureAwait(false).GetAwaiter().GetResult();

                LogExecutingRule(_logger, rule.GetType().Name, entityTypeName, null);

                // Apply rule validation
                var ruleResult = rule.Validate(entity, context);

                // Remove rule resource after completion
                resourceScope?.RemoveResourceAsync(ruleResource).ConfigureAwait(false).GetAwaiter().GetResult();

                LogRuleCompleted(_logger, rule.GetType().Name, ruleResult.Errors.Count, null);

                if (!ruleResult.IsValid)
                {
                    // Merge errors from this rule
                    foreach (var error in ruleResult.Errors)
                    {
                        if (string.IsNullOrEmpty(error.PropertyPath))
                        {
                            builder.WithError(string.Empty, error.Message, error.Code, error.Severity);
                        }
                        else
                        {
                            builder.WithPropertyError(error.Code, error.PropertyName, error.Message, error.Severity);
                        }
                    }

                    // Add exception if any
                    if (ruleResult.Exception != null)
                    {
                        builder.WithException(ruleResult.Exception);
                    }

                    // Stop on first failure if needed
                    if (context.StopOnFirstFailure)
                    {
                        LogValidationFailed(_logger, rule.GetType().Name, null);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogValidationError(_logger, entityTypeName, ex);
                builder.WithError("ValidationException", $"Error validating with rule {rule.RuleId}: {ex.Message}");

                // Stop on exception in strict mode
                if (isStrict && !context.AggregateAllErrors)
                {
                    break;
                }
            }
        }

        // Stop timer and record metrics
        context.StopTimer(operationMetricKey, startTime);

        return builder.Build();
    }

    /// <summary>
    /// Strongly-typed async validation helper with optimized implementation
    /// to avoid circular references and improve performance
    /// </summary>
    private async Task<ICertValidationResult> ValidateAsyncTyped<T>(T entity, CertValidationContext context, CancellationToken cancellationToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        var entityType = typeof(T);
        var entityTypeName = entityType.Name;

        // Convert correlation ID to string consistently
        string correlationId = context.CorrelationId;

        // Combine tokens
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            context.CancellationToken,
            cancellationToken);
        var combinedToken = cts.Token;

        if (combinedToken.IsCancellationRequested)
        {
            LogValidationCancelled(_logger, entityTypeName, null);
            return CreateCancelledResult(correlationId);
        }

        // Create properties dictionary with entity information
        var properties = new Dictionary<string, object>
        {
            ["EntityType"] = entityTypeName,
            ["CorrelationId"] = correlationId,
            ["ValidationMode"] = context.ValidationMode.ToString()
        };

        // Create resource object for this validation operation
        var validationResource = new CertValidationRuleResource(
            $"Validation_{entityTypeName}_{Guid.NewGuid():N}",
            "AsyncValidation",
            $"Async_{entityTypeName}_Validation",
            properties: properties);

        // Try to get or create resource scope
        ICertResourceScope? resourceScope = null;
        if (context.TryGetProperty("ResourceScope", out ICertResourceScope scope))
        {
            resourceScope = scope;
            // Register this async validation operation
            await resourceScope.AddResourceAsync(validationResource, combinedToken).ConfigureAwait(false);
        }

        try
        {
            // Get typed rules with thread safety
            List<ICertValidationRule<T>> typedRules = GetTypedRulesThreadSafe<T>(entityType);

            if (typedRules.Count == 0)
            {
                LogNoRulesForEntity(_logger, entityTypeName, null);
                return CreateSuccessResult(correlationId);
            }

            LogFoundRules(_logger, entityTypeName, typedRules.Count, null);

            var builder = CreateResultBuilder(correlationId);
            var isStrict = context.ValidationMode == PiQApi.Abstractions.Enums.ValidationModeType.Strict;

            // Start tracking validation operation timing
            var operationMetricKey = $"AsyncValidation_{entityTypeName}_{validationResource.ResourceId}";
            var startTime = context.StartTimer(operationMetricKey);

            foreach (var rule in typedRules)
            {
                // Check for cancellation
                if (combinedToken.IsCancellationRequested)
                {
                    LogValidationCancelled(_logger, entityTypeName, null);
                    return CreateCancelledResult(correlationId);
                }

                try
                {
                    // Create rule properties
                    var ruleProperties = new Dictionary<string, object>
                    {
                        ["RuleId"] = rule.RuleId,
                        ["RuleType"] = rule.GetType().Name,
                        ["EntityType"] = entityTypeName
                    };

                    // Create resource for this individual rule execution
                    var ruleResource = new CertValidationRuleResource(
                        $"{rule.RuleId}_{Guid.NewGuid():N}",
                        "AsyncValidationRule",
                        $"Rule_{rule.RuleId}",
                        properties: ruleProperties);

                    // Register rule execution if we have resource scope
                    if (resourceScope != null)
                    {
                        await resourceScope.AddResourceAsync(ruleResource, combinedToken)
                            .ConfigureAwait(false);
                    }

                    LogExecutingRule(_logger, rule.GetType().Name, entityTypeName, null);

                    // Execute the rule asynchronously
                    var ruleResult = await rule.ValidateAsync(entity, context, combinedToken).ConfigureAwait(false);

                    // Remove rule resource after completion
                    if (resourceScope != null)
                    {
                        await resourceScope.RemoveResourceAsync(ruleResource, combinedToken)
                            .ConfigureAwait(false);
                    }

                    LogRuleCompleted(_logger, rule.GetType().Name, ruleResult.Errors.Count, null);

                    if (!ruleResult.IsValid)
                    {
                        // Merge errors from this rule
                        foreach (var error in ruleResult.Errors)
                        {
                            if (string.IsNullOrEmpty(error.PropertyPath))
                            {
                                builder.WithError(string.Empty, error.Message, error.Code, error.Severity);
                            }
                            else
                            {
                                builder.WithPropertyError(error.Code, error.PropertyName, error.Message, error.Severity);
                            }
                        }

                        // Add exception if any
                        if (ruleResult.Exception != null)
                        {
                            builder.WithException(ruleResult.Exception);
                        }

                        // Stop on first failure if needed
                        if (context.StopOnFirstFailure)
                        {
                            LogValidationFailed(_logger, rule.GetType().Name, null);
                            break;
                        }
                    }
                }
                catch (OperationCanceledException) when (combinedToken.IsCancellationRequested)
                {
                    LogValidationCancelled(_logger, entityTypeName, null);
                    return CreateCancelledResult(correlationId);
                }
                catch (Exception ex)
                {
                    LogValidationError(_logger, entityTypeName, ex);
                    builder.WithError("ValidationException", $"Error validating with rule {rule.RuleId}: {ex.Message}");

                    // Stop on exception in strict mode
                    if (isStrict && !context.AggregateAllErrors)
                    {
                        break;
                    }
                }
            }

            // Stop timer and record metrics
            context.StopTimer(operationMetricKey, startTime);

            return builder.Build();
        }
        finally
        {
            // Remove resource if we created one
            if (resourceScope != null)
            {
                await resourceScope.RemoveResourceAsync(validationResource, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Validates an entity using type-specific validation with optimized performance
    /// </summary>
    private ICertValidationResult ValidateWithTypeHandling(object entity, CertValidationContext context, Type entityType, string entityTypeName)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        // Special cases for common types to avoid reflection overhead
        if (entityType == typeof(string))
        {
            return ValidateTypedEntity((string)entity, context, entityType, entityTypeName);
        }
        if (entityType == typeof(CertValidationContext))
        {
            return ValidateTypedEntity((CertValidationContext)entity, context, entityType, entityTypeName);
        }

        // Track this validation operation with a resource ID for diagnostics
        var resourceId = Guid.NewGuid().ToString("N");

        // Try to get operation metrics from context
        if (context.TryGetProperty("MetricsTracker", out ICertOperationMetrics metrics))
        {
            // Register this validation operation as a resource
            metrics.IncrementCounter($"Validation_{entityTypeName}_Started");
        }

        // Get or create a compiled delegate for this entity type
        var validateFunc = _validateFuncCache.GetOrAdd(entityType, type => CreateValidateFunc(type));

        try
        {
            var result = validateFunc(entity, context, entityType, entityTypeName);

            // Record completion metric if metrics are available
            if (context.TryGetProperty("MetricsTracker", out ICertOperationMetrics completionMetrics))
            {
                completionMetrics.IncrementCounter($"Validation_{entityTypeName}_Completed");
                if (!result.IsValid)
                {
                    completionMetrics.IncrementCounter($"Validation_{entityTypeName}_Failed");
                }
            }

            return result;
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap the inner exception for better diagnostics
            LogValidationError(_logger, entityTypeName, ex.InnerException ?? ex);
            return _factory.FromException(ex.InnerException ?? ex);
        }
        catch (Exception ex)
        {
            LogValidationError(_logger, entityTypeName, ex);
            return _factory.FromException(ex);
        }
    }

    /// <summary>
    /// Creates a compiled delegate for validating a specific entity type
    /// </summary>
    private Func<object, CertValidationContext, Type, string, ICertValidationResult> CreateValidateFunc(Type entityType)
    {
        // Use expressions to build a compiled delegate (much faster than reflection)
        var entityParam = Expression.Parameter(typeof(object), "entity");
        var contextParam = Expression.Parameter(typeof(CertValidationContext), "context");
        var typeParam = Expression.Parameter(typeof(Type), "entityType");
        var nameParam = Expression.Parameter(typeof(string), "entityTypeName");

        // Cast the entity to the correct type
        var typedEntity = Expression.Convert(entityParam, entityType);

        // Reference to the ValidateTypedEntity<T> method
        var method = typeof(CertValidationProcessor).GetMethod(
            nameof(ValidateTypedEntity),
            BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(entityType);

        // Build the method call expression
        var call = Expression.Call(
            Expression.Constant(this),
            method,
            typedEntity,
            contextParam,
            typeParam,
            nameParam);

        // Compile the expression to a delegate
        return Expression.Lambda<Func<object, CertValidationContext, Type, string, ICertValidationResult>>(
            call, entityParam, contextParam, typeParam, nameParam).Compile();
    }

    /// <summary>
    /// Creates a compiled delegate for async validation of a specific entity type
    /// </summary>
    private Func<object, CertValidationContext, CancellationToken, Task<ICertValidationResult>> CreateValidateAsyncFunc(Type entityType)
    {
        // Use expressions to build a compiled delegate for async validation
        var entityParam = Expression.Parameter(typeof(object), "entity");
        var contextParam = Expression.Parameter(typeof(CertValidationContext), "context");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        // Cast the entity to the correct type
        var typedEntity = Expression.Convert(entityParam, entityType);

        // Get the generic ValidateAsync method 
        var method = GetType().GetMethod(nameof(ValidateAsyncTyped),
            BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(entityType);

        if (method == null)
        {
            // Fallback to a direct delegate that returns an error
            return (entity, context, ct) => Task.FromResult<ICertValidationResult>(
                _factory.FromException(
                    new InvalidOperationException($"Could not find ValidateAsyncTyped method for type {entityType.Name}")));
        }

        // Build the method call expression
        var call = Expression.Call(
            Expression.Constant(this),
            method,
            typedEntity,
            contextParam,
            ctParam);

        // Compile the expression to a delegate
        return Expression.Lambda<Func<object, CertValidationContext, CancellationToken, Task<ICertValidationResult>>>(
            call, entityParam, contextParam, ctParam).Compile();
    }

    /// <summary>
    /// Gets typed rules for an entity type with proper thread safety
    /// </summary>
    private List<ICertValidationRule<T>> GetTypedRulesThreadSafe<T>(Type entityType) where T : class
    {
        if (!_rules.TryGetValue(entityType, out var rulesList))
        {
            return new List<ICertValidationRule<T>>();
        }

        // Create a thread-safe copy
        lock (rulesList)
        {
            return rulesList.OfType<ICertValidationRule<T>>().ToList();
        }
    }

    /// <summary>
    /// Asynchronously validates an entity with entity type-specific processing
    /// </summary>
    private async Task<ICertValidationResult> ValidateEntityTypeAsync(object entity, CertValidationContext context, Type entityType, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);

        // Create properties for validation tracking
        var properties = new Dictionary<string, object>
        {
            ["EntityType"] = entityType.Name,
            ["CorrelationId"] = context.CorrelationId,
            ["ValidationMode"] = context.ValidationMode.ToString()
        };

        // Create resource object for this validation operation
        var validationResource = new CertValidationRuleResource(
            $"AsyncValidation_{entityType.Name}_{Guid.NewGuid():N}",
            "EntityValidation",
            $"Validation_{entityType.Name}",
            properties: properties);

        // Try to get resource scope from context
        ICertResourceScope? resourceScope = null;
        if (context.TryGetProperty("ResourceScope", out ICertResourceScope scope))
        {
            resourceScope = scope;
            // Register this async validation operation as a resource
            await resourceScope.AddResourceAsync(validationResource, cancellationToken)
                .ConfigureAwait(false);
        }

        try
        {
            // Get or create a compiled delegate for this entity type
            var validateAsyncFunc = _validateAsyncFuncCache.GetOrAdd(entityType, type => CreateValidateAsyncFunc(type));

            var result = await validateAsyncFunc(entity, context, cancellationToken).ConfigureAwait(false);

            // Record completion metric if metrics are available
            if (context.TryGetProperty("MetricsTracker", out ICertOperationMetrics metrics))
            {
                metrics.IncrementCounter($"AsyncValidation_{entityType.Name}_Completed");
                if (!result.IsValid)
                {
                    metrics.IncrementCounter($"AsyncValidation_{entityType.Name}_Failed");
                }
            }

            return result;
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap the inner exception for better diagnostics
            LogValidationError(_logger, entityType.Name, ex.InnerException ?? ex);
            return _factory.FromException(ex.InnerException ?? ex);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            LogValidationCancelled(_logger, entityType.Name, null);
            return CreateCancelledResult(context.CorrelationId);
        }
        catch (Exception ex)
        {
            LogValidationError(_logger, entityType.Name, ex);
            return _factory.FromException(ex);
        }
        finally
        {
            // Remove resource if we created one
            if (resourceScope != null)
            {
                await resourceScope.RemoveResourceAsync(validationResource, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}