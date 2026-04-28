// CertApi.Core/Context/CertOperationContext.Logging.cs
using CertApi.Abstractions.Enums;
using CertApi.Core.Utilities.Disposables;
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Context;

public partial class CertOperationContext
{
    #region LoggerMessage Delegates

    // Operation Start/End Logging
    private static readonly Action<ILogger, string, OperationType, string, Exception?> LogOperationStarted =
        LoggerMessage.Define<string, OperationType, string>(
            LogLevel.Information,
            new EventId(1, nameof(LogOperationStartAsync)),
            "Operation {OperationId} started. Type: {OperationType}, Name: {OperationName}");

    private static readonly Action<ILogger, string, double, string, Exception?> LogOperationCompleted =
        LoggerMessage.Define<string, double, string>(
            LogLevel.Information,
            new EventId(2, nameof(LogOperationEndAsync)),
            "Operation {OperationId} completed successfully in {Duration}ms. CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, double, string, Exception?> LogOperationFailure =
        LoggerMessage.Define<string, double, string>(
            LogLevel.Warning,
            new EventId(3, nameof(LogOperationEndAsync)),
            "Operation {OperationId} completed with errors in {Duration}ms. CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, string, Exception> LogOperationError =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(4, nameof(LogOperationErrorAsync)),
            "Error in operation {OperationId}. CorrelationId: {CorrelationId}");

    // Lifecycle Logging
    private static readonly Action<ILogger, string, OperationType, string, Exception?> LogOperationInitialized =
        LoggerMessage.Define<string, OperationType, string>(
            LogLevel.Debug,
            new EventId(10, nameof(InitializeAsync)),
            "Operation {OperationId} initialized. Type: {OperationType}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, string, Exception?> LogOperationValidated =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(11, nameof(ValidateAsync)),
            "Operation {OperationId} validated");

    private static readonly Action<ILogger, string, string, Exception?> LogChildContextCreated =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(12, nameof(CreateChildContext)),
            "Created child operation {ChildId} from parent {ParentId}");

    // Scope Logging
    private static readonly Action<ILogger, string, string, Exception?> LogScopeStarted =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(13, nameof(CreateScope)),
            "Started operation scope {ScopeName} for operation {OperationId}");

    // Error Handling
    private static readonly Action<ILogger, Exception, string, Exception?> LogOperationEndError =
        LoggerMessage.Define<Exception, string>(
            LogLevel.Warning,
            new EventId(20, "DisposeAsync"),
            "Error logging operation end during disposal: {Error} for {OperationId}");

    #endregion

    /// <summary>
    /// Logs the start of an operation
    /// </summary>
    public async Task LogOperationStartAsync()
    {
        ThrowIfDisposed();

        await _asyncLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Record the operation start
            Metrics.StartTimer("OperationDuration");
            AddProperty("OperationStartUtc", DateTimeOffset.UtcNow);

            // Update state to Running
            if (State is CertOperationState state)
            {
                state.TransitionToRunning();
            }

            // Log operation start
            LogOperationStarted(_logger, Identifier.Id, Identifier.OperationType,
                Identifier.Name, null);
        }
        finally
        {
            _ = _asyncLock.Release();
        }
    }

    /// <summary>
    /// Logs the end of an operation
    /// </summary>
    /// <param name="success">Whether the operation was successful</param>
    public async Task LogOperationEndAsync(bool success)
    {
        ThrowIfDisposed();

        await _asyncLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Record the operation end
            TimeSpan duration = Metrics.StopTimer("OperationDuration");
            AddProperty("OperationEndUtc", DateTimeOffset.UtcNow);
            AddProperty("OperationDurationMs", duration.TotalMilliseconds);
            AddProperty("OperationSuccess", success);

            // Update state based on success
            if (State is CertOperationState state)
            {
                if (success)
                {
                    state.TransitionToComplete();

                    LogOperationCompleted(_logger, Identifier.Id, duration.TotalMilliseconds,
                        CorrelationContext.CorrelationId, null);
                }
                else
                {
                    state.TransitionToFailed();

                    LogOperationFailure(_logger, Identifier.Id, duration.TotalMilliseconds,
                        CorrelationContext.CorrelationId, null);
                }
            }
        }
        finally
        {
            _ = _asyncLock.Release();
        }
    }

    /// <summary>
    /// Logs an operation error
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    public async Task LogOperationErrorAsync(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ThrowIfDisposed();

        await _asyncLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Record error details in properties
            AddProperty("ErrorInfo", exception.Message);
            AddProperty("ErrorType", exception.GetType().Name);
            AddProperty("ErrorTime", DateTimeOffset.UtcNow);

            // Record in metrics
            _ = Metrics.IncrementCounter("ErrorCount");

            // Update state to Failed
            if (State is CertOperationState state)
            {
                state.TransitionToFailed(ErrorCodeType.InternalServerError);
            }

            // Log the error
            LogOperationError(_logger, Identifier.Id, CorrelationContext.CorrelationId, exception);
        }
        finally
        {
            _ = _asyncLock.Release();
        }
    }

    /// <summary>
    /// Creates a logging scope for the current operation
    /// </summary>
    /// <param name="additionalProperties">Additional properties for the scope</param>
    /// <returns>A disposable scope</returns>
    public IDisposable CreateLoggingScope(IDictionary<string, object>? additionalProperties = null)
    {
        ThrowIfDisposed();

        var scopeProperties = new Dictionary<string, object>
        {
            ["OperationId"] = Identifier.Id,
            ["OperationType"] = Identifier.OperationType.ToString(),
            ["CorrelationId"] = CorrelationContext.CorrelationId
        };

        if (additionalProperties != null)
        {
            foreach (var kvp in additionalProperties)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
                {
                    scopeProperties[kvp.Key] = kvp.Value;
                }
            }
        }

        // Some logger implementations might return null from BeginScope
        // Use CertEmptyDisposable as a fallback
        var scope = _logger.BeginScope(scopeProperties);
        return scope ?? CertEmptyDisposable.Instance;
    }

    /// <summary>
    /// Checks if the specified log level is enabled
    /// </summary>
    /// <param name="logLevel">Log level to check</param>
    /// <returns>True if the log level is enabled</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        ThrowIfDisposed();
        return _logger.IsEnabled(logLevel);
    }
}