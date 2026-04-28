// PiQApi.Ews.Core/Interfaces/IEwsServiceFactory.cs
using PiQApi.Abstractions.Service;
using PiQApi.Ews.Core.Interfaces.Context;

namespace PiQApi.Ews.Core.Interfaces
{
    /// <summary>
    /// Factory for creating Exchange Web Service instances
    /// Extends the core service factory interface with EWS-specific functionality
    /// </summary>
    public interface IEwsServiceFactory : ICertServiceFactory
    {
        /// <summary>
        /// Creates a new EWS service
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A new EWS service instance</returns>
        Task<IEwsServiceBase> CreateServiceAsync(IEwsOperationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new EWS service with impersonation
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="impersonatedUser">User to impersonate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A new EWS service instance with impersonation</returns>
        Task<IEwsServiceBase> CreateServiceWithImpersonationAsync(
            IEwsOperationContext context,
            string impersonatedUser,
            CancellationToken cancellationToken = default);
    }
}