// CertApi.Ews.Core/Interfaces/IEwsConnectionManager.cs
using CertApi.Abstractions.Enums;
using CertApi.Ews.Core.Interfaces.Context;
using CertApi.Ews.Core.Models;

namespace CertApi.Ews.Core.Interfaces
{
    /// <summary>
    /// Interface for managing Exchange Web Service connections
    /// </summary>
    public interface IEwsConnectionManager : IAsyncDisposable
    {
        /// <summary>
        /// Gets the current connection state
        /// </summary>
        ConnectionStateType State { get; }

        /// <summary>
        /// Gets the last time a connection was established
        /// </summary>
        DateTimeOffset LastConnectedTime { get; }

        /// <summary>
        /// Gets the current number of connections in the pool
        /// </summary>
        int CurrentPoolSize { get; }

        /// <summary>
        /// Gets whether circuit breaker is open
        /// </summary>
        bool IsCircuitBreakerOpen { get; }

        /// <summary>
        /// Event raised when connection state changes
        /// </summary>
        event EventHandler<EwsConnectionEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Event raised when a connection error occurs
        /// </summary>
        event EventHandler<EwsConnectionEventArgs> ConnectionError;

        /// <summary>
        /// Initializes the connection manager
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Acquires a service base from the pool
        /// </summary>
        /// <param name="context">Operation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Service base implementation</returns>
        Task<IEwsServiceBase> AcquireServiceAsync(IEwsOperationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases a service back to the pool
        /// </summary>
        /// <param name="service">Service to release</param>
        /// <param name="context">Operation context</param>
        Task ReleaseServiceAsync(IEwsServiceBase service, IEwsOperationContext context);

        /// <summary>
        /// Validates a service
        /// </summary>
        /// <param name="service">Service to validate</param>
        /// <param name="context">Operation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if service is valid</returns>
        Task<bool> ValidateServiceAsync(IEwsServiceBase service, IEwsOperationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets the circuit breaker
        /// </summary>
        Task ResetCircuitBreakerAsync();
    }
}
