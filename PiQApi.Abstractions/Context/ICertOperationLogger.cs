// PiQApi.Abstractions/Context/ICertOperationLogger.cs
using Microsoft.Extensions.Logging;

namespace PiQApi.Abstractions.Context;

/// <summary>
/// Defines logging operations for an operation context
/// </summary>
public interface ICertOperationLogger
{
    /// <summary>
    /// Logs the start of an operation
    /// </summary>
    Task LogOperationStartAsync();

    /// <summary>
    /// Logs the end of an operation
    /// </summary>
    /// <param name="success">Whether the operation was successful</param>
    Task LogOperationEndAsync(bool success);

    /// <summary>
    /// Logs an operation error
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    Task LogOperationErrorAsync(Exception exception);

    /// <summary>
    /// Creates a logging scope for the current operation
    /// </summary>
    /// <param name="additionalProperties">Additional properties for the scope</param>
    /// <returns>A disposable scope</returns>
    IDisposable CreateLoggingScope(IDictionary<string, object>? additionalProperties = null);

    /// <summary>
    /// Checks if the specified log level is enabled
    /// </summary>
    /// <param name="logLevel">Log level to check</param>
    /// <returns>True if the log level is enabled</returns>
    bool IsEnabled(LogLevel logLevel);
}
