// PiQApi.Abstractions/Context/IOperationContext.cs

using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Context
{
    /// <summary>
    /// Provides comprehensive contextual information for service operations.
    /// </summary>
    public interface IOperationContext : IAsyncDisposable
    {
        /// <summary>
        /// Gets the unique identifier for this operation
        /// </summary>
        string OperationId { get; }

        /// <summary>
        /// Gets the correlation identifier for distributed tracing
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Gets the type of operation being performed
        /// </summary>
        OperationType OperationType { get; }

        /// <summary>
        /// Gets the time when this operation started
        /// </summary>
        DateTimeOffset StartTime { get; }

        /// <summary>
        /// Gets the timeout period for this operation
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        /// Gets the cancellation token for this operation
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets the current status of this operation
        /// </summary>
        ServiceOperationStatusType Status { get; }

        /// <summary>
        /// Gets the property collection for this operation context
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Initializes the operation context
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the operation context
        /// </summary>
        Task ValidateAsync();

        /// <summary>
        /// Adds a property to the context
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        void AddProperty(string key, object value);

        /// <summary>
        /// Removes a property from the context
        /// </summary>
        /// <param name="key">Property key to remove</param>
        void RemoveProperty(string key);

        /// <summary>
        /// Gets a property of the specified type
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="key">Property key</param>
        /// <returns>Property value</returns>
        T GetProperty<T>(string key);

        /// <summary>
        /// Tries to get a property of the specified type
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="key">Property key</param>
        /// <param name="value">Output property value</param>
        /// <returns>True if property exists and is of correct type</returns>
        bool TryGetProperty<T>(string key, out T value);

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
        /// <param name="ex">The exception that occurred</param>
        Task LogOperationErrorAsync(Exception ex);
    }
}