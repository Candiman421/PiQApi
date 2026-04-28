// CertApi.Abstractions/Context/ICertOperationResources.cs
using CertApi.Abstractions.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Abstractions.Context
{
    /// <summary>
    /// Standardizes resource management within operation context
    /// </summary>
    public interface ICertOperationResources
    {
        /// <summary>
        /// Gets a resource by ID
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resourceId">Resource identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The resource</returns>
        Task<T> GetResourceAsync<T>(string resourceId, CancellationToken cancellationToken = default) where T : class, ICertResource;

        /// <summary>
        /// Tracks a resource
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to track</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The tracked resource</returns>
        Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class, ICertResource;

        /// <summary>
        /// Tries to acquire a resource
        /// </summary>
        /// <param name="resourceId">Resource identifier</param>
        /// <param name="timeout">Timeout for acquisition</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if resource was acquired</returns>
        Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases a resource
        /// </summary>
        /// <param name="resourceId">Resource identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a resource scope for multiple resources
        /// </summary>
        /// <param name="resourceIds">Collection of resource identifiers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A resource scope</returns>
        Task<ICertResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken = default);
    }
}