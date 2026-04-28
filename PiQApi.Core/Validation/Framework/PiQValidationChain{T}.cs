// PiQApi.Core/Validation/Framework/PiQValidationChain{T}.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Utilities.Time;

namespace PiQApi.Core.Validation.Framework;

/// <summary>
/// Chain of validation rules for a specific entity type
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class PiQValidationChain<T> where T : class
{
    private readonly List<IPiQValidationRule<T>> _rules = new();
    private readonly IPiQTimeProvider _timeProvider;
    private readonly IPiQValidationResultFactory _resultFactory;
    private const string MetricPrefix = "ValidationRuleChain_";

    /// <summary>
    /// Gets the number of rules in the chain
    /// </summary>
    public int RuleCount => _rules.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQValidationChain{T}"/> class
    /// </summary>
    /// <param name="resultFactory">Factory to create validation results</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public PiQValidationChain(
        IPiQValidationResultFactory resultFactory,
        IPiQTimeProvider? timeProvider = null)
    {
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        _timeProvider = timeProvider ?? PiQTimeProviderFactory.Current;
    }

    /// <summary>
    /// Adds a rule to the chain
    /// </summary>
    /// <param name="rule">Rule to add</param>
    /// <returns>This chain for method chaining</returns>
    public PiQValidationChain<T> AddRule(IPiQValidationRule<T> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds multiple rules to the chain
    /// </summary>
    /// <param name="rules">Rules to add</param>
    /// <returns>This chain for method chaining</returns>
    public PiQValidationChain<T> AddRules(IEnumerable<IPiQValidationRule<T>> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules.AddRange(rules);
        return this;
    }

    /// <summary>
    /// Clears all rules from the chain
    /// </summary>
    /// <returns>This chain for method chaining</returns>
    public PiQValidationChain<T> ClearRules()
    {
        _rules.Clear();
        return this;
    }

    /// <summary>
    /// Validates an entity synchronously using all rules in the chain
    /// </summary>
    /// <param name="entity">Entity to validate</param>
    /// <param name="context">Validation context</param>
    /// <returns>Validation result</returns>
    public IPiQValidationResult Validate(T entity, PiQValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);
        string correlationId = context.CorrelationId;
        string entityTypeName = typeof(T).Name;
        string metricKey = $"{MetricPrefix}{entityTypeName}";

        // Start metric tracking with timeProvider
        var startTime = _timeProvider.GetTimestamp();
        bool metricsAvailable = context.TryGetProperty("MetricsTracker", out IPiQOperationMetrics metrics);
        if (metricsAvailable)
        {
            metrics.StartTimer(metricKey);
        }

        // Declare builder variable outside try/finally blocks so it's accessible everywhere
        PiQValidationResultBuilder builder = _resultFactory.CreateBuilder(correlationId);
        int errorCount = 0;

        try
        {
            if (_rules.Count == 0)
            {
                return _resultFactory.Success();
            }

            var isStrict = context.ValidationMode == ValidationModeType.Strict;

            foreach (var rule in _rules)
            {
                var result = rule.Validate(entity, context);

                // In strict mode, stop on first error unless ForceAggregateAllErrors is true
                if (!result.IsValid && isStrict && !context.ForceAggregateAllErrors && !context.AggregateAllErrors)
                {
                    return result;
                }

                // Add errors if any
                if (!result.IsValid)
                {
                    builder.Invalid().WithErrors(result.Errors);
                    errorCount += result.Errors.Count;

                    // Add exception if any
                    if (result.Exception != null)
                    {
                        builder.WithException(result.Exception);
                    }
                }
            }

            return builder.Build();
        }
        finally
        {
            // Stop metric tracking
            var elapsed = _timeProvider.GetElapsedTime(startTime);
            if (metricsAvailable && metrics != null)
            {
                metrics.StopTimer(metricKey);
                metrics.RecordTime($"{metricKey}_Duration", elapsed);
                metrics.IncrementCounter($"{metricKey}_Count");
                if (builder.HasErrors)
                {
                    metrics.IncrementCounter($"{metricKey}_Errors", errorCount);
                }
            }
        }
    }

    /// <summary>
    /// Validates an entity asynchronously using all rules in the chain
    /// </summary>
    /// <param name="entity">Entity to validate</param>
    /// <param name="context">Validation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    public async Task<IPiQValidationResult> ValidateAsync(
        T entity,
        PiQValidationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);
        string correlationId = context.CorrelationId;
        string entityTypeName = typeof(T).Name;
        string metricKey = $"{MetricPrefix}Async_{entityTypeName}";

        // Start metric tracking with timeProvider
        var startTime = _timeProvider.GetTimestamp();
        bool metricsAvailable = context.TryGetProperty("MetricsTracker", out IPiQOperationMetrics metrics);
        if (metricsAvailable)
        {
            metrics.StartTimer(metricKey);
        }

        // Declare builder variable outside try/finally blocks so it's accessible everywhere
        PiQValidationResultBuilder builder = _resultFactory.CreateBuilder(correlationId);
        int errorCount = 0;

        try
        {
            // Combine with context token if present
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                context.CancellationToken,
                cancellationToken);
            var combinedToken = cts.Token;

            if (_rules.Count == 0)
            {
                return _resultFactory.Success();
            }

            // Check for cancellation before starting
            if (combinedToken.IsCancellationRequested)
            {
                return _resultFactory.CreateCancelled(correlationId);
            }

            var isStrict = context.ValidationMode == ValidationModeType.Strict;

            try
            {
                foreach (var rule in _rules)
                {
                    // Check for cancellation before each rule
                    combinedToken.ThrowIfCancellationRequested();

                    var result = await rule.ValidateAsync(entity, context, combinedToken).ConfigureAwait(false);

                    // In strict mode, stop on first error unless ForceAggregateAllErrors is true
                    if (!result.IsValid && isStrict && !context.ForceAggregateAllErrors && !context.AggregateAllErrors)
                    {
                        return result;
                    }

                    // Add errors if any
                    if (!result.IsValid)
                    {
                        builder.Invalid().WithErrors(result.Errors);
                        errorCount += result.Errors.Count;

                        // Add exception if any
                        if (result.Exception != null)
                        {
                            builder.WithException(result.Exception);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return _resultFactory.CreateCancelled(correlationId);
            }

            return builder.Build();
        }
        finally
        {
            // Stop metric tracking
            var elapsed = _timeProvider.GetElapsedTime(startTime);
            if (metricsAvailable && metrics != null)
            {
                metrics.StopTimer(metricKey);
                metrics.RecordTime($"{metricKey}_Duration", elapsed);
                metrics.IncrementCounter($"{metricKey}_Count");
                if (builder.HasErrors)
                {
                    metrics.IncrementCounter($"{metricKey}_Errors", errorCount);
                }
            }
        }
    }
}
