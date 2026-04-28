// PiQApi.Abstractions/Service/IPiQServiceFactory.cs
using PiQApi.Abstractions.Context;

namespace PiQApi.Abstractions.Service;

/// <summary>
/// Factory for creating service instances
/// </summary>
public interface IPiQServiceFactory
{
    /// <summary>
    /// Creates a new service
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new service instance</returns>
    Task<IPiQServiceBase> CreateServiceAsync(IPiQOperationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new service with impersonation
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="impersonatedUser">User to impersonate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new service instance with impersonation</returns>
    Task<IPiQServiceBase> CreateServiceWithImpersonationAsync(
        IPiQOperationContext context,
        string impersonatedUser,
        CancellationToken cancellationToken = default);
}