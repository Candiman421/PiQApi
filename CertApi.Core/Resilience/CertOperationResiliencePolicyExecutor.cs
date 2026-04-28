// CertApi.Core/Resilience/CertOperationResiliencePolicyExecutor.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Resilience;
using CertApi.Abstractions.Results;
using CertApi.Abstractions.Utilities.Time;
using CertApi.Core.Resilience.Interfaces;
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Resilience;

/// <summary>
/// Executor for operation resilience policies that handles business operations with metrics tracking
/// </summary>
public class CertOperationResiliencePolicyExecutor : CertResiliencePolicyExecutor, ICertOperationResiliencePolicyExecutor
{
    private readonly ICertOperationContext _operationContext;
    private readonly ICertTimeProvider _timeProvider;
    private readonly ILogger<CertOperationResiliencePolicyExecutor> _logger;

    // LoggerMessage delegate for better performance
    private static readonly Action<ILogger, string, Exception?> _logOperationFailure =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, "BusinessOperationFailed"),
            "Business operation {OperationName} failed");

    /// <summary>
    /// Initializes a new instance of the <see cref="CertOperationResiliencePolicyExecutor"/> class
    /// </summary>
    /// <param name="policyFactory">Policy factory for creating resilience policies</param>
    /// <param name="operationContext">Operation context</param>
    /// <param name="timeProvider">Time provider</param>
    /// <param name="logger">Logger</param>
    /// <param name="loggerFactory">Logger factory</param>
    public CertOperationResiliencePolicyExecutor(
        ICertPolicyFactory policyFactory,
        ICertOperationContext operationContext,
        ICertTimeProvider timeProvider,
        ILogger<CertOperationResiliencePolicyExecutor> logger,
        ILoggerFactory loggerFactory)
        : base(operationContext, policyFactory, logger, loggerFactory, timeProvider)
    {
        _operationContext = operationContext ?? throw new ArgumentNullException(nameof(operationContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a business operation with resilience policy and metrics tracking
    /// </summary>
    /// <typeparam name="T">Type of operation result</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of resilience policy</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    public async Task<T> ExecuteBusinessOperationAsync<T>(Func<Task<T>> operation, ResiliencePolicyType policyType, string operationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        _operationContext.Metrics.StartTimer($"Operation_{operationName}");

        try
        {
            var result = await base.ExecuteAsync(operation, policyType, operationName, cancellationToken).ConfigureAwait(false);
            return result;
        }
        finally
        {
            _operationContext.Metrics.StopTimer($"Operation_{operationName}");
        }
    }

    /// <summary>
    /// Executes a business operation with resilience policy and metrics tracking
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of resilience policy</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteBusinessOperationAsync(Func<Task> operation, ResiliencePolicyType policyType, string operationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        _operationContext.Metrics.StartTimer($"Operation_{operationName}");

        try
        {
            await base.ExecuteAsync(operation, policyType, operationName, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _operationContext.Metrics.StopTimer($"Operation_{operationName}");
        }
    }

    /// <summary>
    /// Executes a business operation with resilience policy, custom exception mapping, and metrics tracking
    /// </summary>
    /// <typeparam name="T">Type of operation result</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="exceptionMapper">Function to map exceptions to operation results</param>
    /// <param name="policyType">Type of resilience policy</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    public async Task<ICertResult<T>> ExecuteWithOperationMappingAsync<T>(
        Func<Task<ICertResult<T>>> operation,
        Func<Exception, ICertOperationContext, ICertResult<T>> exceptionMapper,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentNullException.ThrowIfNull(exceptionMapper, nameof(exceptionMapper));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        _operationContext.Metrics.StartTimer($"Operation_{operationName}");

        try
        {
            return await base.ExecuteAsync(operation, policyType, operationName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logOperationFailure(_logger, operationName, ex);
            return exceptionMapper(ex, _operationContext);
        }
        finally
        {
            _operationContext.Metrics.StopTimer($"Operation_{operationName}");
        }
    }
}
