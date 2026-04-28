// PiQApi.Core/Core/PiQResourceContext.cs
using System.Collections.Concurrent;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Exceptions.Infrastructure;
using PiQApi.Core.Threading;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Core;

/// <summary>
/// Implementation of the resource context that manages resource tracking, acquisition, and release
/// </summary>
public sealed class PiQResourceContext : IPiQResourceContext, IDisposable
{
    private readonly ILogger<PiQResourceContext> _logger;
    private readonly IPiQResourceManager _resourceManager;
    private readonly IPiQExceptionFactory _exceptionFactory;
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _trackedResources = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _resourceLocks = new();
    private bool _disposed;

    /// <summary>
    /// Gets the correlation context
    /// </summary>
    public IPiQCorrelationContext CorrelationContext { get; }

    /// <summary>
    /// Gets the resource metrics
    /// </summary>
    public IPiQResourceMetrics Metrics { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceContext"/> class
    /// </summary>
    /// <param name="correlationContext">The correlation context</param>
    /// <param name="metrics">The resource metrics</param>
    /// <param name="resourceManager">The resource manager</param>
    /// <param name="exceptionFactory">The exception factory</param>
    /// <param name="logger">The logger</param>
    public PiQResourceContext(
        IPiQCorrelationContext correlationContext,
        IPiQResourceMetrics metrics,
        IPiQResourceManager resourceManager,
        IPiQExceptionFactory exceptionFactory,
        ILogger<PiQResourceContext> logger)
    {
        CorrelationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a tracked resource by ID
    /// </summary>
    /// <typeparam name="T">The type of resource</typeparam>
    /// <param name="resourceId">The resource ID</param>
    /// <returns>The tracked resource</returns>
    /// <exception cref="PiQResourceNotFoundException">Thrown when the resource is not found</exception>
    public T GetResource<T>(string resourceId) where T : class, IPiQResource
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));

        var resourceType = typeof(T);
        if (_trackedResources.TryGetValue(resourceType, out var resources) &&
            resources.TryGetValue(resourceId, out var resource) &&
            resource is T typedResource)
        {
            return typedResource;
        }

        throw _exceptionFactory.CreateResourceNotFoundException(typeof(T).Name, resourceId);
    }

    /// <summary>
    /// Tracks a resource
    /// </summary>
    /// <typeparam name="T">The type of resource</typeparam>
    /// <param name="resource">The resource to track</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tracked resource</returns>
    public Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken = default)
        where T : class, IPiQResource
    {
        ArgumentNullException.ThrowIfNull(resource, nameof(resource));
        ObjectDisposedException.ThrowIf(_disposed, this);

        var resourceType = typeof(T);
        var resources = _trackedResources.GetOrAdd(resourceType, _ => new ConcurrentDictionary<string, object>());

        resources[resource.ResourceId] = resource;
        Metrics.IncrementCounter($"Resource_Tracked_{resourceType.Name}");

        // This method doesn't actually require async, but the interface demands it
        // for future extensibility
        return Task.FromResult(resource);
    }

    /// <summary>
    /// Gets all tracked resources of a specific type
    /// </summary>
    /// <typeparam name="T">The type of resources</typeparam>
    /// <returns>A collection of tracked resources</returns>
    public IReadOnlyCollection<T> GetTrackedResources<T>() where T : class, IPiQResource
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var resourceType = typeof(T);
        if (_trackedResources.TryGetValue(resourceType, out var resources))
        {
            return resources.Values.OfType<T>().ToList().AsReadOnly();
        }

        return new List<T>().AsReadOnly();
    }

    /// <summary>
    /// Attempts to acquire a resource
    /// </summary>
    /// <param name="resourceId">The resource ID</param>
    /// <param name="timeout">The timeout for acquisition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the resource was acquired; otherwise, false</returns>
    public async Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));
        ObjectDisposedException.ThrowIf(_disposed, this);

        var semaphore = _resourceLocks.GetOrAdd(resourceId, _ => new SemaphoreSlim(1, 1));

        var acquired = await semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);

        if (acquired)
        {
            Metrics.IncrementCounter($"Resource_Acquired_{resourceId}");
        }

        return acquired;
    }

    /// <summary>
    /// Releases a resource
    /// </summary>
    /// <param name="resourceId">The resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when the resource is released</returns>
    public Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_resourceLocks.TryGetValue(resourceId, out var semaphore))
        {
            semaphore.Release();
            Metrics.IncrementCounter($"Resource_Released_{resourceId}");
        }

        // This method doesn't actually require async, but the interface demands it
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a resource scope
    /// </summary>
    /// <param name="resourceIds">The resource IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A resource scope</returns>
    /// <exception cref="PiQResourceLockedException">Thrown when a resource cannot be acquired</exception>
    public async Task<IPiQResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resourceIds, nameof(resourceIds));
        ObjectDisposedException.ThrowIf(_disposed, this);

        var scopeResources = resourceIds.ToList();
        foreach (var id in scopeResources)
        {
            var acquired = await TryAcquireResourceAsync(id, TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

            if (!acquired)
            {
                // Release any resources already acquired
                foreach (var acquiredId in scopeResources.TakeWhile(r => r != id))
                {
                    await ReleaseResourceAsync(acquiredId, cancellationToken).ConfigureAwait(false);
                }

                throw _exceptionFactory.CreateResourceLockedException(id);
            }
        }

        return new PiQAsyncResourceScope(this, scopeResources, _logger);
    }

    /// <summary>
    /// Disposes managed resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var resourceLock in _resourceLocks.Values)
            {
                resourceLock.Dispose();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
