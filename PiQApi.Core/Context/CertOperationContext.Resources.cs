// PiQApi.Core/Context/CertOperationContext.Resources.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Core.Context;

public partial class CertOperationContext
{
    /// <summary>
    /// Gets a resource by ID
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <param name="resourceId">Resource identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The resource</returns>
    public Task<T> GetResourceAsync<T>(string resourceId, CancellationToken cancellationToken = default) where T : class, ICertResource
    {
        ThrowIfDisposed();
        return _resources.GetResourceAsync<T>(resourceId, cancellationToken);
    }

    /// <summary>
    /// Tracks a resource
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <param name="resource">Resource to track</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tracked resource</returns>
    public Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class, ICertResource
    {
        ThrowIfDisposed();
        return _resources.TrackResourceAsync(resource, cancellationToken);
    }

    /// <summary>
    /// Tries to acquire a resource
    /// </summary>
    /// <param name="resourceId">Resource identifier</param>
    /// <param name="timeout">Timeout for acquisition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if resource was acquired</returns>
    public Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _resources.TryAcquireResourceAsync(resourceId, timeout, cancellationToken);
    }

    /// <summary>
    /// Releases a resource
    /// </summary>
    /// <param name="resourceId">Resource identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _resources.ReleaseResourceAsync(resourceId, cancellationToken);
    }

    /// <summary>
    /// Creates a resource scope for multiple resources
    /// </summary>
    /// <param name="resourceIds">Collection of resource identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A resource scope</returns>
    public Task<ICertResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _resources.CreateResourceScopeAsync(resourceIds, cancellationToken);
    }
}