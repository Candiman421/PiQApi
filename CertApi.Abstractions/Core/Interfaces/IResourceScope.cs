// CertApi.Abstractions/Core/Interfaces/IResourceScope.cs
namespace CertApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Represents a scope for managing resources
    /// </summary>
    public interface IResourceScope : IAsyncDisposable
    {
        /// <summary>
        /// Gets the unique identifier for this resource scope
        /// </summary>
        string ScopeId { get; }

        /// <summary>
        /// Gets the creation timestamp for this scope
        /// </summary>
        DateTimeOffset Created { get; }

        /// <summary>
        /// Adds a resource to the scope
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The added resource</returns>
        Task<T> AddResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Removes a resource from the scope
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RemoveResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Disposes the scope and cleans up all resources
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CleanupAsync(CancellationToken cancellationToken = default);
    }
}