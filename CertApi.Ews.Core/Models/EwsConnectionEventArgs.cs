// CertApi.Ews.Core/Models/EwsConnectionEventArgs.cs
using CertApi.Abstractions.Enums;

namespace CertApi.Ews.Core.Models
{
    /// <summary>
    /// Event arguments for connection events with immutable properties for thread safety
    /// </summary>
    public class EwsConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EwsConnectionEventArgs"/> class
        /// </summary>
        /// <param name="oldState">Old connection state</param>
        /// <param name="newState">New connection state</param>
        /// <param name="correlationId">Correlation ID associated with the event</param>
        /// <param name="exception">Optional exception that occurred</param>
        public EwsConnectionEventArgs(
            ConnectionStateType oldState,
            ConnectionStateType newState,
            string? correlationId = null,
            Exception? exception = null)
        {
            OldState = oldState;
            NewState = newState;
            CorrelationId = correlationId;
            Exception = exception;
        }

        /// <summary>
        /// Gets the old connection state
        /// </summary>
        public ConnectionStateType OldState { get; }

        /// <summary>
        /// Gets the new connection state
        /// </summary>
        public ConnectionStateType NewState { get; }

        /// <summary>
        /// Gets the exception that occurred (if any)
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the correlation ID associated with the event
        /// </summary>
        public string? CorrelationId { get; }

        /// <summary>
        /// Creates a new event args instance with updated exception information
        /// </summary>
        /// <param name="exception">The exception to include</param>
        /// <returns>A new event args instance with the same state but different exception</returns>
        public EwsConnectionEventArgs WithException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return new EwsConnectionEventArgs(OldState, NewState, CorrelationId, exception);
        }

        /// <summary>
        /// Creates a new event args instance with updated correlation ID
        /// </summary>
        /// <param name="correlationId">The correlation ID to include</param>
        /// <returns>A new event args instance with the same state but different correlation ID</returns>
        public EwsConnectionEventArgs WithCorrelationId(string correlationId)
        {
            ArgumentException.ThrowIfNullOrEmpty(correlationId);
            return new EwsConnectionEventArgs(OldState, NewState, correlationId, Exception);
        }
    }
}