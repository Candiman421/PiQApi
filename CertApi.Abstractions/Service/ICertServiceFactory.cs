// CertApi.Abstractions/Service/ICertServiceFactory.cs
using CertApi.Abstractions.Context;

namespace CertApi.Abstractions.Service;

/// <summary>
/// Factory for creating service instances
/// </summary>
public interface ICertServiceFactory
{
    /// <summary>
    /// Creates a new service
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new service instance</returns>
    Task<ICertServiceBase> CreateServiceAsync(ICertOperationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new service with impersonation
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="impersonatedUser">User to impersonate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new service instance with impersonation</returns>
    Task<ICertServiceBase> CreateServiceWithImpersonationAsync(
        ICertOperationContext context,
        string impersonatedUser,
        CancellationToken cancellationToken = default);
}