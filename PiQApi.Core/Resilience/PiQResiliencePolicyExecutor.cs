// PiQApi.Core/Resilience/PiQResiliencePolicyExecutor.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Resilience;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Resilience.Interfaces;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Implementation of the resilience policy executor
/// </summary>
public class PiQResiliencePolicyExecutor : IPiQResiliencePolicyExecutor
{
    private readonly IPiQOperationContext _operationContext;
    private readonly IPiQPolicyFactory _policyFactory;
    private readonly ILogger<PiQResiliencePolicyExecutor> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPiQTimeProvider _timeProvider;

    // Define LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, ResiliencePolicyType, string, string, Exception?> LogExecutingOperation =
        LoggerMessage.Define<string, ResiliencePolicyType, string, string>(
            LogLevel.Debug,
            new EventId(100, "ExecutingOperation"),
            "Executing operation {OperationName} with policy {PolicyType}, OperationId: {OperationId}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, ResiliencePolicyType, string, Exception> LogOperationError =
        LoggerMessage.Define<string, ResiliencePolicyType, string>(
            LogLevel.Error,
            new EventId(101, "OperationError"),
            "Error executing operation {OperationName} with policy {PolicyType}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, Exception?> LogStartingPolicyExecution =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(102, "StartingPolicyExecution"),
            "Starting policy execution for {OperationName}");

    /// <summary>
    /// Initializes a new instance of the PiQResiliencePolicyExecutor class
    /// </summary>
    /// <param name="operationContext">Operation context</param>
    /// <param name="policyFactory">Policy factory</param>
    /// <param name="logger">Logger</param>
    /// <param name="loggerFactory">Logger factory</param>
    /// <param name="timeProvider">Time provider</param>
    public PiQResiliencePolicyExecutor(
        IPiQOperationContext operationContext,
        IPiQPolicyFactory policyFactory,
        ILogger<PiQResiliencePolicyExecutor> logger,
        ILoggerFactory loggerFactory,
        IPiQTimeProvider? timeProvider = null)
    {
        _operationContext = operationContext ?? throw new ArgumentNullException(nameof(operationContext));
        _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _timeProvider = timeProvider ?? new PiQSystemTimeProvider();
    }

    /// <summary>
    /// Executes an operation with resilience policy
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Resilience policy type</param>
    /// <param name="operationName">Operation name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        string correlationId = _operationContext.CorrelationContext.CorrelationId;

        LogExecutingOperation(_logger, operationName, policyType, _operationContext.Identifier.Id, correlationId, null);

        var metric = $"Policy_{policyType}_{operationName}";
        _operationContext.Metrics.StartTimer(metric);
        _operationContext.Metrics.IncrementCounter(metric);

        // Use _timeProvider to measure the operation time
        var startTimestamp = _timeProvider.GetTimestamp();

        try
        {
            var policy = _policyFactory.CreatePolicy<T>(policyType);

            // Create a typed policy logger using _loggerFactory
            var policyLogger = _loggerFactory.CreateLogger($"PolicyExecution.{policyType}");
            LogStartingPolicyExecution(policyLogger, operationName, null);

            var wrappedOperation = new Func<CancellationToken, Task<T>>(async token =>
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    token, _operationContext.CancellationToken);

                return await operation().ConfigureAwait(false);
            });

            return await policy.ExecuteAsync(wrappedOperation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogOperationError(_logger, operationName, policyType, correlationId, ex);

            // Add correlation data to exception if not already present
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(correlationId))
            {
                ex.Data["CorrelationId"] = correlationId;
                ex.Data["OperationId"] = _operationContext.Identifier.Id;
                ex.Data["OperationName"] = operationName;
            }

            // Record failure in metrics
            _operationContext.Metrics.IncrementCounter($"Policy_{policyType}_Failures");

            throw;
        }
        finally
        {
            // Use _timeProvider to calculate elapsed time instead of Stopwatch
            var elapsed = _timeProvider.GetElapsedTime(startTimestamp);
            _operationContext.Metrics.StopTimer(metric);
            _operationContext.Metrics.RecordTime($"{metric}_Duration", elapsed);
        }
    }

    /// <summary>
    /// Executes an operation that returns no result
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Resilience policy type</param>
    /// <param name="operationName">Operation name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteAsync(
        Func<Task> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        await ExecuteAsync<object>(
            async () =>
            {
                await operation().ConfigureAwait(false);
                return null!;
            },
            policyType,
            operationName,
            cancellationToken).ConfigureAwait(false);
    }
}