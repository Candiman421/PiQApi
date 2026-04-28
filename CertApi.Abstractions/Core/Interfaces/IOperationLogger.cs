// CertApi.Abstractions/Core/Interfaces/IOperationLogger.cs
using Microsoft.Extensions.Logging;

namespace CertApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Provides logging capabilities for tracking operations throughout the system
    /// </summary>
    public interface IOperationLogger
    {
        /// <summary>
        /// Logs the start of an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="properties">Additional properties to log</param>
        Task LogOperationStartAsync(string operationName, string correlationId, IDictionary<string, object>? properties = null);

        /// <summary>
        /// Logs the end of an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="success">Whether the operation was successful</param>
        Task LogOperationEndAsync(string operationName, string correlationId, TimeSpan duration, bool success = true);

        /// <summary>
        /// Logs an error that occurred during an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="exception">Exception that occurred</param>
        Task LogOperationErrorAsync(string operationName, string correlationId, Exception exception);

        /// <summary>
        /// Creates a logging scope for an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="properties">Additional properties for the scope</param>
        /// <returns>A disposable scope</returns>
        Task<IDisposable> BeginScopeAsync(string operationName, string correlationId, IDictionary<string, object>? properties = null);

        /// <summary>
        /// Creates a scoped logger for a specific operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <returns>A scoped logger</returns>
        Task<IOperationLogger> CreateScopedLoggerAsync(string operationName);

        /// <summary>
        /// Checks if the specified log level is enabled
        /// </summary>
        /// <param name="logLevel">Log level to check</param>
        /// <returns>True if the log level is enabled</returns>
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// Begins a property scope for logging
        /// </summary>
        /// <param name="properties">Properties for the scope</param>
        /// <returns>A disposable scope</returns>
        IDisposable BeginPropertyScope(IDictionary<string, object> properties);
    }
}