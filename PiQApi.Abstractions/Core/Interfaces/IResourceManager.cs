// PiQApi.Abstractions/Core/Interfaces/IResourceManager.cs
namespace PiQApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Manages tracked resources and resource scopes
    /// </summary>
    public interface IResourceManager : IAsyncDisposable
    {
        /// <summary>
        /// Creates a new resource scope
        /// </summary>
        /// <param name="scopeId">Optional identifier for the scope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A resource scope</returns>
        Task<IResourceScope> CreateScopeAsync(string? scopeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans up resources in a specific scope
        /// </summary>
        /// <param name="scopeId">Identifier of the scope to clean up</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CleanupScopeAsync(string scopeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans up all resource scopes
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CleanupAllScopesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracks a resource
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to track</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The tracked resource</returns>
        Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Removes a tracked resource
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RemoveTrackedResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Checks if a resource exists
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the resource exists</returns>
        Task<bool> ResourceExistsAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Gets all tracked resources of a specific type
        /// </summary>
        /// <typeparam name="T">Type of resources to get</typeparam>
        /// <returns>Collection of tracked resources</returns>
        IReadOnlyCollection<T> GetTrackedResources<T>() where T : class;

        /// <summary>
        /// Tries to acquire a resource
        /// </summary>
        /// <param name="resourceId">Identifier of the resource</param>
        /// <param name="timeout">Timeout for acquisition attempt</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the resource was acquired</returns>
        Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases a previously acquired resource
        /// </summary>
        /// <param name="resourceId">Identifier of the resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a resource scope for multiple resources
        /// </summary>
        /// <param name="resourceIds">Collection of resource identifiers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A resource scope</returns>
        Task<IResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken = default);
    }
}