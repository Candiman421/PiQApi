// PiQApi.Core/Resilience/PiQResiliencePolicy.cs
using PiQApi.Abstractions.Resilience;
using Polly;
using Polly.Timeout;
using Microsoft.Extensions.Logging;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Utilities.Time;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Implementation of IPiQResiliencePolicy wrapping Polly policies
/// </summary>
public class PiQResiliencePolicy : IPiQResiliencePolicy
{
    private readonly IAsyncPolicy _policy;
    private readonly ILogger<PiQResiliencePolicy> _logger;
    private readonly IPiQCorrelationContext? _correlationContext;
    private readonly IPiQTimeProvider _timeProvider;

    // Define LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, Exception?> LogExecutingOperation =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(100, "ExecutingOperation"),
            "Executing operation with resilience policy, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, Exception> LogOperationFailed =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(101, "OperationFailed"),
            "Operation failed with resilience policy, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, Exception?> LogOperationSucceeded =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(102, "OperationSucceeded"),
            "Operation succeeded with resilience policy, CorrelationId: {CorrelationId}");

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResiliencePolicy"/> class
    /// </summary>
    /// <param name="policy">Polly policy to wrap</param>
    /// <param name="logger">Logger</param>
    /// <param name="correlationContext">Optional correlation context</param>
    /// <param name="timeProvider">Provider for time-related operations</param>
    public PiQResiliencePolicy(
        IAsyncPolicy policy,
        ILogger<PiQResiliencePolicy> logger,
        IPiQCorrelationContext? correlationContext = null,
        IPiQTimeProvider? timeProvider = null)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationContext = correlationContext;
        _timeProvider = timeProvider ?? new PiQSystemTimeProvider();
    }

    /// <summary>
    /// Executes an operation with the policy
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Never create a new correlation ID - either use existing context's ID or empty string
        string correlationId = _correlationContext?.CorrelationId ?? string.Empty;

        // Create context with correlation ID and other metadata
        var policyContext = new Polly.Context();

        // Only add correlation ID if it exists
        if (!string.IsNullOrEmpty(correlationId))
        {
            policyContext["CorrelationId"] = correlationId;
        }

        // Add all other properties from correlation context
        if (_correlationContext != null)
        {
            foreach (var prop in _correlationContext.Properties)
            {
                if (!policyContext.TryGetValue(prop.Key, out _))
                {
                    policyContext[prop.Key] = prop.Value;
                }
            }
        }

        // Track execution time using the injected time provider
        var startTime = _timeProvider.GetTimestamp();

        try
        {
            LogExecutingOperation(_logger, correlationId, null);

            // Check if our policy is a properly typed policy
            if (_policy is IAsyncPolicy<T> typedPolicy)
            {
                var result = await typedPolicy.ExecuteAsync(
                    async (ctx, ct) => await operation(ct).ConfigureAwait(false),
                    policyContext,
                    cancellationToken).ConfigureAwait(false);

                LogOperationSucceeded(_logger, correlationId, null);
                return result;
            }
            else
            {
                // If the policy is not generic, use a wrapper to execute and cast the result
                var result = await _policy.ExecuteAsync(
                    async (ctx, ct) => await operation(ct).ConfigureAwait(false),
                    policyContext,
                    cancellationToken).ConfigureAwait(false);

                LogOperationSucceeded(_logger, correlationId, null);
                return result;
            }
        }
        catch (TimeoutRejectedException ex)
        {
            var duration = _timeProvider.GetElapsedTime(startTime);
            LogOperationFailed(_logger, correlationId, ex);
            var timeoutEx = new TimeoutException($"Operation timed out after {duration.TotalMilliseconds}ms. Correlation ID: {correlationId}", ex);

            // Ensure correlation ID is included in exception data
            if (!string.IsNullOrEmpty(correlationId))
            {
                timeoutEx.Data["CorrelationId"] = correlationId;
                timeoutEx.Data["Duration"] = duration.TotalMilliseconds;
            }

            throw timeoutEx;
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            LogOperationFailed(_logger, correlationId, ex);
            // Add correlation ID to exception data if not already present
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(correlationId))
            {
                ex.Data["CorrelationId"] = correlationId;
            }
            throw;
        }
        catch (Exception ex)
        {
            LogOperationFailed(_logger, correlationId, ex);
            // Add correlation ID to exception data if not already present
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(correlationId))
            {
                ex.Data["CorrelationId"] = correlationId;
            }
            throw;
        }
    }

    /// <summary>
    /// Executes an operation with the policy
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Never create a new correlation ID - either use existing context's ID or empty string
        string correlationId = _correlationContext?.CorrelationId ?? string.Empty;

        // Create context with correlation ID and other metadata
        var policyContext = new Polly.Context();

        // Only add correlation ID if it exists
        if (!string.IsNullOrEmpty(correlationId))
        {
            policyContext["CorrelationId"] = correlationId;
        }

        // Add all other properties from correlation context
        if (_correlationContext != null)
        {
            foreach (var prop in _correlationContext.Properties)
            {
                if (!policyContext.TryGetValue(prop.Key, out _))
                {
                    policyContext[prop.Key] = prop.Value;
                }
            }
        }

        // Track execution time using the injected time provider
        var startTime = _timeProvider.GetTimestamp();

        try
        {
            LogExecutingOperation(_logger, correlationId, null);

            await _policy.ExecuteAsync(
                async (ctx, ct) => await operation(ct).ConfigureAwait(false),
                policyContext,
                cancellationToken).ConfigureAwait(false);

            LogOperationSucceeded(_logger, correlationId, null);
        }
        catch (TimeoutRejectedException ex)
        {
            var duration = _timeProvider.GetElapsedTime(startTime);
            LogOperationFailed(_logger, correlationId, ex);
            var timeoutEx = new TimeoutException($"Operation timed out after {duration.TotalMilliseconds}ms. Correlation ID: {correlationId}", ex);

            // Ensure correlation ID is included in exception data
            if (!string.IsNullOrEmpty(correlationId))
            {
                timeoutEx.Data["CorrelationId"] = correlationId;
                timeoutEx.Data["Duration"] = duration.TotalMilliseconds;
            }

            throw timeoutEx;
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            LogOperationFailed(_logger, correlationId, ex);
            // Add correlation ID to exception data if not already present
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(correlationId))
            {
                ex.Data["CorrelationId"] = correlationId;
            }
            throw;
        }
        catch (Exception ex)
        {
            LogOperationFailed(_logger, correlationId, ex);
            // Add correlation ID to exception data if not already present
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(correlationId))
            {
                ex.Data["CorrelationId"] = correlationId;
            }
            throw;
        }
    }
}
