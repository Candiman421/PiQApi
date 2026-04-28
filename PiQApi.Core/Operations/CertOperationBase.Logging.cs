// PiQApi.Core/Operations/CertOperationBase.Logging.cs
using PiQApi.Core.Utilities.Disposables;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Operations;

public abstract partial class CertOperationBase
{
    #region LoggerMessage Delegates

    private static readonly Action<ILogger, string, Exception?> LogInitializing =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, nameof(InitializeAsync)),
            "Initializing operation: {OperationId}");

    private static readonly Action<ILogger, string, Exception?> LogValidatingState =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, nameof(ValidateStateAsync)),
            "Validating operation state: {OperationId}");

    private static readonly Action<ILogger, string, Exception?> LogCheckingOperational =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(3, nameof(IsOperationalAsync)),
            "Checking if operation is operational: {OperationId}");

    private static readonly Action<ILogger, string, Exception?> LogCleaning =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(4, nameof(CleanupAsync)),
            "Cleaning up operation: {OperationId}");

    private static readonly Action<ILogger, string, long, Exception?> LogDisposeCompleted =
        LoggerMessage.Define<string, long>(
            LogLevel.Debug,
            new EventId(5, nameof(DisposeAsync)),
            "Operation disposed: {OperationId}, Total Duration: {DurationMs}ms");

    private static readonly Action<ILogger, string, string, Exception?> LogOperationSuccess =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(6, "OperationSuccess"),
            "Operation {OperationId} completed successfully. CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogOperationFailure =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(7, "OperationFailure"),
            "Operation {OperationId} failed. Status: {Status}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, string, Exception> LogOperationError =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(8, "OperationError"),
            "Error in operation {OperationId}. CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, string, Exception?> LogLockAcquisitionFailed =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(9, nameof(DisposeAsync)),
            "Could not acquire lock during disposal for {OperationId}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, string, Exception> LogOperationEndError =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(10, nameof(DisposeAsync)),
            "Error logging operation end during disposal for {OperationId}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, Exception, string, string, Exception?> LogOperationLogError =
        LoggerMessage.Define<Exception, string, string>(
            LogLevel.Warning,
            new EventId(11, nameof(LogErrorAsync)),
            "Error logging operation error: {Error} for {OperationId}, CorrelationId: {CorrelationId}");

    #endregion

    /// <summary>
    /// Creates a new logging scope with additional data
    /// </summary>
    /// <param name="additionalData">Additional data to include in the scope</param>
    /// <returns>A disposable logging scope</returns>
    protected IDisposable CreateLoggingScope(IDictionary<string, object>? additionalData = null)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["OperationId"] = OperationId,
            ["CorrelationId"] = CorrelationId
        };

        if (additionalData != null)
        {
            foreach (var item in additionalData)
            {
                scopeData[item.Key] = item.Value;
            }
        }

        return _logger.BeginScope(scopeData) ?? new CertDisposableAction(() => { });
    }

    /// <summary>
    /// Creates a new operation scope
    /// </summary>
    /// <param name="operationName">Name of the operation for the scope</param>
    /// <returns>A disposable operation scope</returns>
    protected IDisposable CreateScope(string operationName)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName);
        return Context.CreateScope(operationName) ?? new CertDisposableAction(() => { });
    }

    /// <summary>
    /// Logs an operation success
    /// </summary>
    /// <param name="additionalData">Additional data to include in the log</param>
    protected void LogSuccess(IDictionary<string, object>? additionalData = null)
    {
        using var scope = CreateLoggingScope(additionalData);
        LogOperationSuccess(_logger, OperationId, CorrelationId, null);
    }

    /// <summary>
    /// Logs an operation failure
    /// </summary>
    /// <param name="additionalData">Additional data to include in the log</param>
    protected void LogFailure(IDictionary<string, object>? additionalData = null)
    {
        using var scope = CreateLoggingScope(additionalData);
        string status = Context?.State?.Status.ToString() ?? "Unknown";
        LogOperationFailure(_logger, OperationId, status, CorrelationId, null);
    }

    /// <summary>
    /// Logs an operation error with additional data
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="additionalData">Additional data to include in the log</param>
    protected async Task LogErrorAsync(Exception exception, IDictionary<string, object>? additionalData = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Ensure exception has correlation ID
        if (!exception.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationId))
        {
            exception.Data["CorrelationId"] = CorrelationId;
        }

        using var scope = CreateLoggingScope(additionalData);
        LogOperationError(_logger, OperationId, CorrelationId, exception);

        // Also log the error to the context for centralized error handling
        await Context.LogOperationErrorAsync(exception).ConfigureAwait(false);
    }

    /// <summary>
    /// Logs an error
    /// </summary>
    /// <param name="ex">Exception to log</param>
    /// <returns>A task representing the asynchronous operation</returns>
    protected async Task LogErrorAsync(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        try
        {
            // Ensure exception has correlation ID
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationId))
            {
                ex.Data["CorrelationId"] = CorrelationId;
            }

            await Context.LogOperationErrorAsync(ex).ConfigureAwait(false);
        }
        catch (Exception logEx)
        {
            // CA1031 justification: We're catching all exceptions here to prevent exceptions
            // in error logging from affecting the operation. This is a last-resort logging method.
            LogOperationLogError(_logger, logEx, OperationId, CorrelationId, null);
        }
    }
}