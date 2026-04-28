// PiQApi.Core/Validation/Framework/PiQValidationChain.Sync.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;

namespace PiQApi.Core.Validation.Framework;

public partial class PiQValidationChain
{
    /// <summary>
    /// Validates a strongly-typed entity using all processors in the chain
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    /// <param name="entity">Entity to validate</param>
    /// <param name="context">Validation context</param>
    /// <returns>Validation result</returns>
    public IPiQValidationResult Validate<T>(T entity, PiQValidationContext context) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);
        string correlationId = context.CorrelationId;
        string entityTypeName = typeof(T).Name;
        string metricKey = $"ValidationChain_{_chainName}_{entityTypeName}";

        // Start metric tracking with timeProvider
        var startTime = _timeProvider.GetTimestamp();
        bool metricsAvailable = context.TryGetProperty("MetricsTracker", out IPiQOperationMetrics metrics);
        if (metricsAvailable)
        {
            metrics.StartTimer(metricKey);
        }

        using (_validationChainScope(_logger, _chainName))
        {
            _logStartingValidation(_logger, entityTypeName, null);

            if (_processors.Count == 0)
            {
                _logNoProcessors(_logger, null);
                StopMetrics(startTime, metricKey, metrics, metricsAvailable, 0);
                return _resultFactory.Success();
            }

            var result = _resultFactory.Success();

            // Determine if we should aggregate all errors or stop on first error in strict mode
            var shouldAggregateAllErrors = context.AggregateAllErrors || context.ForceAggregateAllErrors;

            _logValidationMode(_logger, context.ValidationMode, shouldAggregateAllErrors, null);

            foreach (var processor in _processors)
            {
                _logExecutingProcessor(_logger, processor.GetType().Name, null);

                var processorResult = processor.Validate(entity, context);

                _logProcessorReturned(_logger, processorResult.Errors.Count, null);

                // Merge errors from this processor
                // Since we're now using the interface, we'll use the factory for combining
                result = (IPiQValidationResult)_resultFactory.Combine(result, processorResult);

                // Check for errors in strict mode to potentially stop processing
                // Only apply strict mode stopping if we're not in a test that needs to aggregate all errors
                if (!shouldAggregateAllErrors &&
                    context.ValidationMode == ValidationModeType.Full &&
                    HasErrorsOfSeverity(result, ValidationSeverityType.Error))
                {
                    _logStoppingValidation(_logger, null);
                    break;
                }
            }

            _logValidationComplete(_logger, result.Errors.Count, null);
            StopMetrics(startTime, metricKey, metrics, metricsAvailable, result.Errors.Count);
            return result;
        }
    }

    /// <summary>
    /// Validates an entity synchronously using all processors in the chain
    /// </summary>
    /// <param name="entity">Entity to validate</param>
    /// <param name="context">Validation context</param>
    /// <returns>Validation result</returns>
    public IPiQValidationResult Validate(object entity, PiQValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);
        string correlationId = context.CorrelationId;
        string entityTypeName = entity.GetType().Name;
        string metricKey = $"ValidationChain_{_chainName}_{entityTypeName}";

        // Start metric tracking with timeProvider
        var startTime = _timeProvider.GetTimestamp();
        bool metricsAvailable = context.TryGetProperty("MetricsTracker", out IPiQOperationMetrics metrics);
        if (metricsAvailable)
        {
            metrics.StartTimer(metricKey);
        }

        try
        {
            if (_processors.Count == 0)
            {
                _logNoProcessors(_logger, null);
                return _resultFactory.Success();
            }

            // Create a builder to accumulate results
            var builder = _resultFactory.CreateBuilder(correlationId);
            var isStrict = context.ValidationMode == ValidationModeType.Strict;

            foreach (var processor in _processors)
            {
                var result = processor.Validate(entity, context);

                // In strict mode, stop on first error unless ForceAggregateAllErrors is true
                if (!result.IsValid && isStrict && !context.ForceAggregateAllErrors && !context.AggregateAllErrors)
                {
                    _logStoppingValidation(_logger, null);
                    return result;
                }

                // Add errors if any
                if (!result.IsValid)
                {
                    _ = builder.Invalid().WithErrors(result.Errors);

                    // Add exception if any
                    if (result.Exception != null)
                    {
                        _ = builder.WithException(result.Exception);
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
            }
        }
    }

    /// <summary>
    /// Helper method to check if a validation result has errors of a certain severity
    /// </summary>
    private static bool HasErrorsOfSeverity(IPiQValidationResult result, ValidationSeverityType severity)
    {
        return result.Errors.Any(e => e.Severity == severity);
    }
}
