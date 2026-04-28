// PiQApi.Abstractions/Core/IPiQResourceContext.cs
namespace PiQApi.Abstractions.Core;

/// <summary>
/// Provides context for resource operations and management
/// </summary>
public interface IPiQResourceContext : IPiQCorrelationAware
{
    /// <summary>
    /// Gets a resource by ID
    /// </summary>
    /// <typeparam name="T">Type of resource to retrieve</typeparam>
    /// <param name="resourceId">ID of the resource to retrieve</param>
    /// <returns>The requested resource</returns>
    /// <exception>Thrown when the resource is not found</exception>
    T GetResource<T>(string resourceId) where T : class, IPiQResource;

    /// <summary>
    /// Creates a resource scope that automatically acquires and releases multiple resources
    /// </summary>
    /// <param name="resourceIds">IDs of resources to include in the scope</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A resource scope that will release all resources when disposed</returns>
    /// <exception>Thrown when a resource cannot be acquired</exception>
    Task<IPiQResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the metrics for resource operations
    /// </summary>
    IPiQResourceMetrics Metrics { get; }

    /// <summary>
    /// Tracks a resource in the context
    /// </summary>
    /// <typeparam name="T">Type of resource to track</typeparam>
    /// <param name="resource">Resource to track</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tracked resource</returns>
    Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class, IPiQResource;

    /// <summary>
    /// Gets all tracked resources of a specific type
    /// </summary>
    /// <typeparam name="T">Type of resources to retrieve</typeparam>
    /// <returns>Collection of tracked resources</returns>
    IReadOnlyCollection<T> GetTrackedResources<T>() where T : class, IPiQResource;

    /// <summary>
    /// Attempts to acquire a resource lock
    /// </summary>
    /// <param name="resourceId">ID of the resource to lock</param>
    /// <param name="timeout">Maximum time to wait for the lock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the lock was acquired; otherwise, false</returns>
    Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a previously acquired resource lock
    /// </summary>
    /// <param name="resourceId">ID of the resource to release</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken = default);
}
