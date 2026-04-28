// PiQApi.Core/Validation/Framework/PiQValidationChain.Async.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;

namespace PiQApi.Core.Validation.Framework;

public partial class PiQValidationChain
{
    /// <summary>
    /// Validates a strongly-typed entity asynchronously using all processors in the chain
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    /// <param name="entity">Entity to validate</param>
    /// <param name="context">Validation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    public async Task<IPiQValidationResult> ValidateAsync<T>(T entity, PiQValidationContext context,
        CancellationToken cancellationToken = default) where T : class
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

        using (_asyncValidationChainScope(_logger, _chainName))
        {
            _logStartingAsyncValidation(_logger, entityTypeName, null);

            if (_processors.Count == 0)
            {
                _logNoProcessorsAsync(_logger, null);
                StopMetrics(startTime, metricKey, metrics, metricsAvailable, 0);
                return _resultFactory.Success();
            }

            var result = _resultFactory.Success();

            // Determine if we should aggregate all errors or stop on first error in strict mode
            var shouldAggregateAllErrors = context.AggregateAllErrors || context.ForceAggregateAllErrors;

            _logAsyncValidationMode(_logger, context.ValidationMode, shouldAggregateAllErrors, null);

            foreach (var processor in _processors)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StopMetrics(startTime, metricKey, metrics, metricsAvailable, 0);
                    return _resultFactory.CreateCancelled(correlationId);
                }

                _logExecutingAsyncProcessor(_logger, processor.GetType().Name, null);

                var processorResult = await processor.ValidateAsync(entity, context, cancellationToken).ConfigureAwait(false);

                _logAsyncProcessorReturned(_logger, processorResult.Errors.Count, null);

                // Merge errors from this processor
                result = (IPiQValidationResult)_resultFactory.Combine(result, processorResult);

                // Check for errors in strict mode to potentially stop processing
                // Only apply strict mode stopping if we're not in a test that needs to aggregate all errors
                if (!shouldAggregateAllErrors &&
                    context.ValidationMode == ValidationModeType.Full &&
                    HasErrorsOfSeverity(result, ValidationSeverityType.Error))
                {
                    _logStoppingAsyncValidation(_logger, null);
                    break;
                }
            }

            _logAsyncValidationComplete(_logger, result.Errors.Count, null);
            StopMetrics(startTime, metricKey, metrics, metricsAvailable, result.Errors.Count);
            return result;
        }
    }

    /// <summary>
    /// Validates an entity asynchronously using all processors in the chain
    /// </summary>
    /// <param name="entity">Entity to validate</param>
    /// <param name="context">Validation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    public async Task<IPiQValidationResult> ValidateAsync(
        object entity,
        PiQValidationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(context);
        string correlationId = context.CorrelationId;
        string entityTypeName = entity.GetType().Name;
        string metricKey = $"ValidationChain_Async_{_chainName}_{entityTypeName}";

        // Start metric tracking with timeProvider
        var startTime = _timeProvider.GetTimestamp();
        bool metricsAvailable = context.TryGetProperty("MetricsTracker", out IPiQOperationMetrics metrics);
        if (metricsAvailable)
        {
            metrics.StartTimer(metricKey);
        }

        try
        {
            // Combine with context token if present
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                context.CancellationToken,
                cancellationToken);
            var combinedToken = cts.Token;

            if (_processors.Count == 0)
            {
                _logNoProcessorsAsync(_logger, null);
                return _resultFactory.Success();
            }

            // Check for cancellation before starting
            if (combinedToken.IsCancellationRequested)
            {
                _logValidationCancelled(_logger, null);
                return _resultFactory.CreateCancelled(correlationId);
            }

            // Create a builder to accumulate results
            var builder = _resultFactory.CreateBuilder(correlationId);
            var isStrict = context.ValidationMode == ValidationModeType.Strict;

            try
            {
                foreach (var processor in _processors)
                {
                    // Check for cancellation before each processor
                    combinedToken.ThrowIfCancellationRequested();

                    var result = await processor.ValidateAsync(entity, context, combinedToken).ConfigureAwait(false);

                    // In strict mode, stop on first error unless ForceAggregateAllErrors is true
                    if (!result.IsValid && isStrict && !context.ForceAggregateAllErrors && !context.AggregateAllErrors)
                    {
                        _logStoppingAsyncValidation(_logger, null);
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
            }
            catch (OperationCanceledException)
            {
                _logValidationCancelledDuringExecution(_logger, null);
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
            }
        }
    }

    /// <summary>
    /// Helper method to stop metrics tracking
    /// </summary>
    private void StopMetrics(TimeSpan startTime, string metricKey, IPiQOperationMetrics? metrics, bool metricsAvailable, int errorCount)
    {
        var elapsed = _timeProvider.GetElapsedTime(startTime);
        if (metricsAvailable && metrics != null)
        {
            metrics.StopTimer(metricKey);
            metrics.RecordTime($"{metricKey}_Duration", elapsed);
            metrics.IncrementCounter($"{metricKey}_Count");
            metrics.IncrementCounter($"{metricKey}_Errors", errorCount);
        }
    }
}
