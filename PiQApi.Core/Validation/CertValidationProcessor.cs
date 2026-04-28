// PiQApi.Core/Validation/CertValidationProcessor.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Core.Validation
{
    /// <summary>
    /// Core implementation of validation processor
    /// </summary>
    public partial class CertValidationProcessor : ICertValidationProcessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICertValidationResultFactory _validationResultFactory;
        private readonly ILogger<CertValidationProcessor> _logger;
        private readonly ConcurrentDictionary<Type, List<object>> _registeredRules = new();

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, Type, int, Exception?> LogRulesRegistered =
            LoggerMessage.Define<Type, int>(
                LogLevel.Debug,
                new EventId(1, nameof(RegisterRules)),
                "Registered {Count} rules for type {EntityType}");

        private static readonly Action<ILogger, Type, int, Exception?> LogValidationStarted =
            LoggerMessage.Define<Type, int>(
                LogLevel.Debug,
                new EventId(2, nameof(ValidateInternal)),
                "Starting validation for type {EntityType} with {RuleCount} rules");

        private static readonly Action<ILogger, Type, Exception?> LogClearRules =
            LoggerMessage.Define<Type>(
                LogLevel.Debug,
                new EventId(3, nameof(ClearRules)),
                "Cleared all validation rules for type {EntityType}");

        /// <summary>
        /// Initializes a new instance of the <see cref="CertValidationProcessor"/> class
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving validation rules</param>
        /// <param name="validationResultFactory">Factory for creating validation results</param>
        /// <param name="logger">Logger</param>
        public CertValidationProcessor(
            IServiceProvider serviceProvider,
            ICertValidationResultFactory validationResultFactory,
            ILogger<CertValidationProcessor> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _validationResultFactory = validationResultFactory ?? throw new ArgumentNullException(nameof(validationResultFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers validation rules for a type
        /// </summary>
        /// <typeparam name="T">Type to register rules for</typeparam>
        /// <param name="rules">Rules to register</param>
        public void RegisterRules<T>(IEnumerable<ICertValidationRule<T>> rules) where T : class
        {
            ArgumentNullException.ThrowIfNull(rules);

            var rulesList = rules.ToList();
            if (rulesList.Count == 0)
            {
                return;
            }

            // Add rules to the dictionary
            _registeredRules.AddOrUpdate(
                typeof(T),
                _ => rulesList.Cast<object>().ToList(),
                (_, existingRules) =>
                {
                    existingRules.AddRange(rulesList.Cast<object>());
                    return existingRules;
                });

            LogRulesRegistered(_logger, typeof(T), rulesList.Count, null);
        }

        /// <summary>
        /// Clears all registered rules
        /// </summary>
        public void ClearRules()
        {
            var clearedTypes = new List<Type>(_registeredRules.Keys);
            _registeredRules.Clear();

            foreach (var type in clearedTypes)
            {
                LogClearRules(_logger, type, null);
            }
        }

        /// <summary>
        /// Gets the count of registered rules for a type
        /// </summary>
        /// <typeparam name="T">Type to get rule count for</typeparam>
        /// <returns>Number of registered rules</returns>
        public int GetRuleCount<T>() where T : class
        {
            return _registeredRules.TryGetValue(typeof(T), out var rules) ? rules.Count : 0;
        }

        /// <summary>
        /// Validates an object
        /// </summary>
        /// <typeparam name="T">Type of object to validate</typeparam>
        /// <param name="entity">Object to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        public ICertValidationResult Validate<T>(T entity, ICertValidationContext context) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            return ValidateInternal(entity, context);
        }

        /// <summary>
        /// Validates an object asynchronously
        /// </summary>
        /// <typeparam name="T">Type of object to validate</typeparam>
        /// <param name="entity">Object to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public Task<ICertValidationResult> ValidateAsync<T>(T entity, ICertValidationContext context, CancellationToken cancellationToken = default) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            return ValidateInternalAsync(entity, context, cancellationToken);
        }

        /// <summary>
        /// Validates a non-generic object
        /// </summary>
        /// <param name="entity">Object to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        public ICertValidationResult Validate(object entity, ICertValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            // Use reflection to call the generic method with the correct type
            var entityType = entity.GetType();
            var methodInfo = typeof(CertValidationProcessor).GetMethod(
                nameof(ValidateInternal),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var genericMethod = methodInfo?.MakeGenericMethod(entityType);
            return (ICertValidationResult?)(genericMethod?.Invoke(this, new object[] { entity, context }))
                ?? _validationResultFactory.CreateInvalidResult(
                    context,
                    "UnableToValidate",
                    $"Unable to validate object of type {entityType.Name}");
        }

        /// <summary>
        /// Validates a non-generic object asynchronously
        /// </summary>
        /// <param name="entity">Object to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public async Task<ICertValidationResult> ValidateAsync(object entity, ICertValidationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            // Use reflection to call the generic method with the correct type
            var entityType = entity.GetType();
            var methodInfo = typeof(CertValidationProcessor).GetMethod(
                nameof(ValidateInternalAsync),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var genericMethod = methodInfo?.MakeGenericMethod(entityType);
            if (genericMethod != null)
            {
                var task = (Task<ICertValidationResult>?)genericMethod.Invoke(
                    this, new object[] { entity, context, cancellationToken });

                if (task != null)
                {
                    return await task.ConfigureAwait(false);
                }
            }

            // Fallback if reflection fails
            return _validationResultFactory.CreateInvalidResult(
                context,
                "UnableToValidate",
                $"Unable to validate object of type {entityType.Name}");
        }

        /// <summary>
        /// Implementation of the validation logic for an entity
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        private ICertValidationResult ValidateInternal<T>(T entity, ICertValidationContext context) where T : class
        {
            // Check if we have any rules for this type
            if (!_registeredRules.TryGetValue(typeof(T), out var rules) || rules.Count == 0)
            {
                // Try to resolve rules from DI
                var resolvedRules = _serviceProvider.GetServices<ICertValidationRule<T>>().ToList();
                if (resolvedRules.Count > 0)
                {
                    var rulesList = resolvedRules.ToList();
                    _registeredRules.AddOrUpdate(
                        typeof(T),
                        _ => rulesList.Cast<object>().ToList(),
                        (_, existingRules) =>
                        {
                            existingRules.AddRange(rulesList.Cast<object>());
                            return existingRules;
                        });
                    rules = resolvedRules.Cast<object>().ToList();
                }
            }

            // If still no rules found, return success
            if (rules == null || rules.Count == 0)
            {
                return _validationResultFactory.Success();
            }

            LogValidationStarted(_logger, typeof(T), rules.Count, null);

            // Apply each rule
            var errors = new List<CertValidationError>();
            foreach (var rule in rules)
            {
                if (rule is ICertValidationRule<T> typedRule)
                {
                    var result = typedRule.Validate(entity, context);
                    if (!result.IsValid)
                    {
                        errors.AddRange(result.Errors);
                    }
                }
            }

            // Return a result based on errors found
            return errors.Count > 0
                ? _validationResultFactory.FromErrors(errors)
                : _validationResultFactory.Success();
        }

        /// <summary>
        /// Implementation of the asynchronous validation logic for an entity
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        private async Task<ICertValidationResult> ValidateInternalAsync<T>(
            T entity,
            ICertValidationContext context,
            CancellationToken cancellationToken) where T : class
        {
            // Check if cancellation has been requested
            if (cancellationToken.IsCancellationRequested)
            {
                return _validationResultFactory.CreateCancelled(context.CorrelationId);
            }

            // Check if we have any rules for this type
            if (!_registeredRules.TryGetValue(typeof(T), out var rules) || rules.Count == 0)
            {
                // Try to resolve rules from DI
                var resolvedRules = _serviceProvider.GetServices<ICertValidationRule<T>>().ToList();
                if (resolvedRules.Count > 0)
                {
                    var rulesList = resolvedRules.ToList();
                    _registeredRules.AddOrUpdate(
                        typeof(T),
                        _ => rulesList.Cast<object>().ToList(),
                        (_, existingRules) =>
                        {
                            existingRules.AddRange(rulesList.Cast<object>());
                            return existingRules;
                        });
                    rules = resolvedRules.Cast<object>().ToList();
                }
            }

            // If still no rules found, return success
            if (rules == null || rules.Count == 0)
            {
                return _validationResultFactory.Success();
            }

            LogValidationStarted(_logger, typeof(T), rules.Count, null);

            // Apply each rule asynchronously
            var errors = new List<CertValidationError>();
            var asyncRuleResults = new List<Task<ICertValidationResult>>();

            // Separate sync and async rules
            foreach (var rule in rules)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return _validationResultFactory.CreateCancelled(context.CorrelationId);
                }

                if (rule is ICertAsyncValidationRule<T> asyncRule)
                {
                    asyncRuleResults.Add(asyncRule.ValidateAsync(entity, context, cancellationToken));
                }
                else if (rule is ICertValidationRule<T> syncRule)
                {
                    var result = syncRule.Validate(entity, context);
                    if (!result.IsValid)
                    {
                        errors.AddRange(result.Errors);
                    }
                }
            }

            // Process async rule results
            if (asyncRuleResults.Count > 0)
            {
                var asyncResults = await Task.WhenAll(asyncRuleResults).ConfigureAwait(false);
                foreach (var result in asyncResults)
                {
                    if (!result.IsValid)
                    {
                        errors.AddRange(result.Errors);
                    }
                }
            }

            // Check for cancellation again
            if (cancellationToken.IsCancellationRequested)
            {
                return _validationResultFactory.CreateCancelled(context.CorrelationId);
            }

            // Return a result based on errors found
            return errors.Count > 0
                ? _validationResultFactory.FromErrors(errors)
                : _validationResultFactory.Success();
        }

        /// <summary>
        /// Validates an entity using specific validation rules
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="ruleTypes">Types of validation rules to use</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        public ICertValidationResult ValidateWithRules<T>(
            T entity,
            IEnumerable<Type> ruleTypes,
            ICertValidationContext context) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(ruleTypes);
            ArgumentNullException.ThrowIfNull(context);

            var rulesList = ruleTypes.ToList();
            if (rulesList.Count == 0)
            {
                return _validationResultFactory.Success();
            }

            var errors = new List<CertValidationError>();

            // Instantiate and apply each rule
            foreach (var ruleType in rulesList)
            {
                try
                {
                    // Try to get the rule from the service provider
                    var rule = _serviceProvider.GetService(ruleType);

                    if (rule == null)
                    {
                        // Try to create an instance directly if not registered
                        rule = Activator.CreateInstance(ruleType);
                    }

                    if (rule is ICertValidationRule<T> typedRule)
                    {
                        var result = typedRule.Validate(entity, context);
                        if (!result.IsValid)
                        {
                            errors.AddRange(result.Errors);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating or applying validation rule of type {RuleType}", ruleType.Name);
                    errors.Add(new CertValidationError(
                        string.Empty,
                        $"Error applying validation rule: {ex.Message}",
                        "ValidationRuleError"));
                }
            }

            // Return a result based on errors found
            return errors.Count > 0
                ? _validationResultFactory.FromErrors(errors)
                : _validationResultFactory.Success();
        }
    }
}
