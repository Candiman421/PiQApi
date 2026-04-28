// PiQApi.Core/Resilience/CertServiceResiliencePolicyExecutor.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Resilience;
using PiQApi.Abstractions.Results;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Resilience.Interfaces;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Executor for service resilience policies that handles service operations with metrics tracking
/// </summary>
public class CertServiceResiliencePolicyExecutor : CertResiliencePolicyExecutor, ICertServiceResiliencePolicyExecutor
{
    private readonly ICertOperationContext _operationContext;
    private readonly ICertTimeProvider _timeProvider;
    private readonly ILogger<CertServiceResiliencePolicyExecutor> _logger;

    // LoggerMessage delegate for better performance
    private static readonly Action<ILogger, string, Exception?> _logServiceFailure =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, "ServiceOperationFailed"),
            "Service operation {OperationName} failed");

    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceResiliencePolicyExecutor"/> class
    /// </summary>
    /// <param name="policyFactory">Policy factory for creating resilience policies</param>
    /// <param name="operationContext">Operation context</param>
    /// <param name="timeProvider">Time provider</param>
    /// <param name="logger">Logger</param>
    /// <param name="loggerFactory">Logger factory</param>
    public CertServiceResiliencePolicyExecutor(
        ICertPolicyFactory policyFactory,
        ICertOperationContext operationContext,
        ICertTimeProvider timeProvider,
        ILogger<CertServiceResiliencePolicyExecutor> logger,
        ILoggerFactory loggerFactory)
        : base(operationContext, policyFactory, logger, loggerFactory, timeProvider)
    {
        _operationContext = operationContext ?? throw new ArgumentNullException(nameof(operationContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a service operation with resilience policy and metrics tracking
    /// </summary>
    /// <typeparam name="T">Type of operation result</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of resilience policy</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    public async Task<T> ExecuteServiceAsync<T>(Func<Task<T>> operation, ResiliencePolicyType policyType, string operationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        _operationContext.Metrics.StartTimer($"Service_{operationName}");

        try
        {
            var result = await base.ExecuteAsync(operation, policyType, operationName, cancellationToken).ConfigureAwait(false);
            return result;
        }
        finally
        {
            _operationContext.Metrics.StopTimer($"Service_{operationName}");
        }
    }

    /// <summary>
    /// Executes a service operation with resilience policy and metrics tracking
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of resilience policy</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteServiceAsync(Func<Task> operation, ResiliencePolicyType policyType, string operationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        _operationContext.Metrics.StartTimer($"Service_{operationName}");

        try
        {
            await base.ExecuteAsync(operation, policyType, operationName, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _operationContext.Metrics.StopTimer($"Service_{operationName}");
        }
    }

    /// <summary>
    /// Executes a service operation with resilience policy, custom exception mapping, and metrics tracking
    /// </summary>
    /// <typeparam name="T">Type of operation result</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="exceptionMapper">Function to map exceptions to service results</param>
    /// <param name="policyType">Type of resilience policy</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service result of the operation</returns>
    public async Task<ICertServiceResult<T>> ExecuteWithServiceMappingAsync<T>(
        Func<Task<ICertServiceResult<T>>> operation,
        Func<Exception, ICertOperationContext, ICertServiceResult<T>> exceptionMapper,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));
        ArgumentNullException.ThrowIfNull(exceptionMapper, nameof(exceptionMapper));
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        _operationContext.Metrics.StartTimer($"Service_{operationName}");

        try
        {
            return await base.ExecuteAsync(operation, policyType, operationName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logServiceFailure(_logger, operationName, ex);
            return exceptionMapper(ex, _operationContext);
        }
        finally
        {
            _operationContext.Metrics.StopTimer($"Service_{operationName}");
        }
    }
}
