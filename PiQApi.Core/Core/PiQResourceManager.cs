// PiQApi.Core/Core/PiQResourceManager.cs
using PiQApi.Abstractions.Core;
using PiQApi.Core.Threading;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using PiQApi.Core.Exceptions.Infrastructure;

namespace PiQApi.Core.Core;

/// <summary>
/// Implementation of the resource manager for tracking resources
/// </summary>
public class PiQResourceManager : IPiQResourceManager
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _resourceScopes = new();
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _typedResources = new();
    private readonly ConcurrentDictionary<string, IPiQAsyncLock> _resourceLocks = new();
    private readonly ILogger<PiQResourceManager> _logger;
    private readonly IPiQAsyncLockFactory _lockFactory;
    private readonly IPiQCorrelationContext? _correlationContext;
    private readonly IPiQAsyncLock _managerLock;
    private bool _isDisposed;

    #region LoggerMessage Delegates

    private static readonly Action<ILogger, string, string, Exception?> LogCreatingScope =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(CreateScopeAsync)),
            "[{CorrelationId}] Creating resource scope {ScopeId}");

    private static readonly Action<ILogger, string, string, Exception?> LogCleaningUpScope =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(2, nameof(CleanupScopeAsync)),
            "[{CorrelationId}] Cleaning up resource scope {ScopeId}");

    private static readonly Action<ILogger, string, string, Exception?> LogTrackingResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(3, nameof(TrackResourceAsync)),
            "[{CorrelationId}] Tracking resource of type {ResourceType}");

    private static readonly Action<ILogger, string, string, Exception?> LogRemovingResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(4, nameof(RemoveTrackedResourceAsync)),
            "[{CorrelationId}] Removing tracked resource of type {ResourceType}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogAcquiringResource =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Debug,
            new EventId(5, nameof(TryAcquireResourceAsync)),
            "[{CorrelationId}] Attempting to acquire resource {ResourceId} with timeout {Timeout}");

    private static readonly Action<ILogger, string, string, Exception?> LogResourceAcquired =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(6, "ResourceAcquired"),
            "[{CorrelationId}] Successfully acquired resource {ResourceId}");

    private static readonly Action<ILogger, string, string, Exception?> LogResourceAcquisitionTimedOut =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(7, "ResourceAcquisitionTimedOut"),
            "[{CorrelationId}] Timed out while acquiring resource {ResourceId}");

    private static readonly Action<ILogger, string, string, Exception?> LogReleasingResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(8, nameof(ReleaseResourceAsync)),
            "[{CorrelationId}] Releasing resource {ResourceId}");

    private static readonly Action<ILogger, string, string, Exception> LogErrorAcquiringResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(9, nameof(TryAcquireResourceAsync)),
            "[{CorrelationId}] Error acquiring resource {ResourceId}");

    private static readonly Action<ILogger, string, int, Exception?> LogCreatingResourceScope =
        LoggerMessage.Define<string, int>(
            LogLevel.Debug,
            new EventId(10, nameof(CreateResourceScopeAsync)),
            "[{CorrelationId}] Creating resource scope for {ResourceCount} resources");

    private static readonly Action<ILogger, string, Exception?> LogNoLockFound =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(11, "NoLockFound"),
            "No lock found for resource {ResourceId} when attempting to release");

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceManager"/> class
    /// </summary>
    /// <param name="logger">Logger for tracking operations</param>
    /// <param name="lockFactory">Factory for creating async locks</param>
    /// <param name="correlationContext">Correlation context for tracking operations</param>
    public PiQResourceManager(
        ILogger<PiQResourceManager>? logger = null,
        IPiQAsyncLockFactory? lockFactory = null,
        IPiQCorrelationContext? correlationContext = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PiQResourceManager>.Instance;
        _correlationContext = correlationContext;

        // If lock factory is not provided, create a new one
        _lockFactory = lockFactory ?? new PiQAsyncLockFactory();
        _managerLock = _lockFactory.Create();
    }

    /// <summary>
    /// Creates a new resource scope
    /// </summary>
    /// <param name="scopeId">Optional identifier for the scope</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A resource scope</returns>
    public async Task<IPiQResourceScope> CreateScopeAsync(string? scopeId = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        // Generate a scope ID if one wasn't provided
        var effectiveScopeId = scopeId ?? Guid.NewGuid().ToString();

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "CreateScope");
        _correlationContext?.AddProperty("ScopeId", effectiveScopeId);

        var correlationId = _correlationContext?.CorrelationId ?? string.Empty;
        LogCreatingScope(_logger, correlationId, effectiveScopeId, null);

        // Use the async lock to ensure thread-safe scope creation
        using var lockReleaser = await _managerLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        // Add the scope to the dictionary if it doesn't already exist
        var resources = _resourceScopes.GetOrAdd(effectiveScopeId, _ => new ConcurrentDictionary<string, object>());

        // Create and return the resource scope
        var scope = new PiQResourceScope(this, effectiveScopeId, _logger);
        return scope;
    }

    /// <summary>
    /// Cleans up resources in a specific scope
    /// </summary>
    /// <param name="scopeId">Identifier of the scope to clean up</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CleanupScopeAsync(string scopeId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(scopeId);

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "CleanupScope");
        _correlationContext?.AddProperty("ScopeId", scopeId);

        var correlationId = _correlationContext?.CorrelationId ?? string.Empty;
        LogCleaningUpScope(_logger, correlationId, scopeId, null);

        // Use the async lock to ensure thread-safe scope cleanup
        using var lockReleaser = await _managerLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        // Remove the scope and get its resources
        if (_resourceScopes.TryRemove(scopeId, out var resources))
        {
            // Process in parallel
            var cleanupTasks = resources.Values
                .OfType<IAsyncDisposable>()
                .Select(resource => resource.DisposeAsync().AsTask());

            // Await all cleanup tasks
            await Task.WhenAll(cleanupTasks).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Cleans up all resource scopes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CleanupAllScopesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "CleanupAllScopes");

        // Use the async lock to ensure thread-safe operations
        using var lockReleaser = await _managerLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        // Get all scope IDs
        var scopeIds = _resourceScopes.Keys.ToList();

        // Clean up each scope
        foreach (var scopeId in scopeIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CleanupScopeAsync(scopeId, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Tracks a resource
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <param name="resource">Resource to track</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tracked resource</returns>
    public async Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(resource);

        var resourceType = typeof(T);

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "TrackResource");
        _correlationContext?.AddProperty("ResourceType", resourceType.Name);

        var correlationId = _correlationContext?.CorrelationId ?? string.Empty;
        LogTrackingResource(_logger, correlationId, resourceType.Name, null);

        // Use the async lock to ensure thread-safe resource tracking
        using var lockReleaser = await _managerLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        // Get or create the dictionary for this resource type
        var resources = _typedResources.GetOrAdd(resourceType, _ => new ConcurrentDictionary<string, object>());

        // Add the resource using a suitable key
        string resourceKey = resource is IPiQResource piqResource
            ? piqResource.ResourceId
            : resource.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);

        resources[resourceKey] = resource;

        return resource;
    }

    /// <summary>
    /// Removes a tracked resource
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <param name="resource">Resource to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RemoveTrackedResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(resource);

        var resourceType = typeof(T);

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "RemoveTrackedResource");
        _correlationContext?.AddProperty("ResourceType", resourceType.Name);

        var correlationId = _correlationContext?.CorrelationId ?? string.Empty;
        LogRemovingResource(_logger, correlationId, resourceType.Name, null);

        // Use the async lock to ensure thread-safe resource removal
        using var lockReleaser = await _managerLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        // Try to get the dictionary for this resource type
        if (_typedResources.TryGetValue(resourceType, out var resources))
        {
            // Get the resource key
            string resourceKey = resource is IPiQResource piqResource
                ? piqResource.ResourceId
                : resource.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);

            // Remove the resource
            resources.TryRemove(resourceKey, out _);
        }
    }


    /// <summary>
    /// Checks if a resource exists
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <param name="resource">Resource to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the resource exists</returns>
    public async Task<bool> ResourceExistsAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(resource);

        var resourceType = typeof(T);

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "ResourceExists");
        _correlationContext?.AddProperty("ResourceType", resourceType.Name);

        // Create a cancellable task to ensure async behavior
        return await Task.Run(() =>
        {
            // Try to get the dictionary for this resource type
            if (_typedResources.TryGetValue(resourceType, out var resources))
            {
                // Get the resource key
                string resourceKey = resource is IPiQResource piqResource
                    ? piqResource.ResourceId
                    : resource.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Check if the resource exists
                return resources.ContainsKey(resourceKey);
            }

            return false;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all tracked resources of a specific type
    /// </summary>
    /// <typeparam name="T">Type of resources to get</typeparam>
    /// <returns>Collection of tracked resources</returns>
    public IReadOnlyCollection<T> GetTrackedResources<T>() where T : class
    {
        ThrowIfDisposed();

        var resourceType = typeof(T);

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "GetTrackedResources");
        _correlationContext?.AddProperty("ResourceType", resourceType.Name);

        // Try to get the dictionary for this resource type
        if (_typedResources.TryGetValue(resourceType, out var resources))
        {
            // Return all resources of the specified type
            return resources.Values.OfType<T>().ToList().AsReadOnly();
        }

        return Array.Empty<T>();
    }

    /// <summary>
    /// Tries to acquire a resource
    /// </summary>
    /// <param name="resourceId">Identifier of the resource</param>
    /// <param name="timeout">Timeout for acquisition attempt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the resource was acquired</returns>
    public async Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(resourceId);

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "TryAcquireResource");
        _correlationContext?.AddProperty("ResourceId", resourceId);

        var correlationId = _correlationContext?.CorrelationId ?? string.Empty;
        LogAcquiringResource(_logger, correlationId, resourceId, timeout.ToString(), null);

        // Get or create the lock for this resource
        var resourceLock = _resourceLocks.GetOrAdd(resourceId, _ => _lockFactory.Create());

        // Try to acquire the lock
        try
        {
            var lockReleaser = await resourceLock.TryAcquireAsync(timeout, cancellationToken).ConfigureAwait(false);

            // If we got a releaser, the lock was acquired
            bool acquired = lockReleaser != null;

            if (acquired)
            {
                LogResourceAcquired(_logger, correlationId, resourceId, null);
            }
            else
            {
                LogResourceAcquisitionTimedOut(_logger, correlationId, resourceId, null);
            }

            return acquired;
        }
        catch (OperationCanceledException)
        {
            // Let cancellation exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            LogErrorAcquiringResource(_logger, correlationId, resourceId, ex);
            return false;
        }
    }

    /// <summary>
    /// Releases a previously acquired resource
    /// </summary>
    /// <param name="resourceId">Identifier of the resource</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(resourceId);

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "ReleaseResource");
        _correlationContext?.AddProperty("ResourceId", resourceId);

        var correlationId = _correlationContext?.CorrelationId ?? string.Empty;
        LogReleasingResource(_logger, correlationId, resourceId, null);

        // Try to get the lock for this resource
        if (_resourceLocks.TryGetValue(resourceId, out var resourceLock))
        {
            // Add await to fix the CS1998 warning
            await resourceLock.ReleaseAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // Add another operation that uses await to fix the warning
            // If the lock doesn't exist, log that information
            await Task.CompletedTask.ConfigureAwait(false);
            LogNoLockFound(_logger, resourceId, null);
        }
    }

    /// <summary>
    /// Creates a resource scope for multiple resources
    /// </summary>
    /// <param name="resourceIds">Collection of resource identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A resource scope</returns>
    public async Task<IPiQResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(resourceIds);

        var resourceIdList = resourceIds.ToList();

        // Track operation in correlation context
        _correlationContext?.AddProperty("Operation", "CreateResourceScope");
        _correlationContext?.AddProperty("ResourceCount", resourceIdList.Count);

        var correlationId = _correlationContext?.CorrelationId ?? string.Empty;
        LogCreatingResourceScope(_logger, correlationId, resourceIdList.Count, null);

        // Create a resource scope with proper disposal handling
#pragma warning disable CA2000 // Suppressed: Scope object is either returned to the caller who must dispose it,
        // or explicitly disposed in the error paths below
        var scope = await CreateScopeAsync(null, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA2000

        var acquiredResources = new List<string>();

        try
        {
            // Try to acquire each resource
            foreach (var resourceId in resourceIdList)
            {
                if (await TryAcquireResourceAsync(resourceId, TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false))
                {
                    acquiredResources.Add(resourceId);
                }
                else
                {
                    // Release any resources already acquired
                    foreach (var acquiredResourceId in acquiredResources)
                    {
                        await ReleaseResourceAsync(acquiredResourceId, cancellationToken).ConfigureAwait(false);
                    }

                    // Cleanup the scope
                    await scope.CleanupAsync(cancellationToken).ConfigureAwait(false);
                    await scope.DisposeAsync().ConfigureAwait(false);

                    // Throw a resource locked exception
                    throw new PiQResourceLockedException(
                        resourceId,
                        "Another operation currently has a lock on this resource");
                }
            }

            return scope;
        }
        catch (PiQResourceLockedException)
        {
            // Let resource locked exceptions propagate
            throw;
        }
        catch (Exception ex)
        {
            // Release any resources already acquired
            foreach (var acquiredResourceId in acquiredResources)
            {
                await ReleaseResourceAsync(acquiredResourceId, cancellationToken).ConfigureAwait(false);
            }

            // Cleanup the scope
            await scope.CleanupAsync(cancellationToken).ConfigureAwait(false);
            await scope.DisposeAsync().ConfigureAwait(false);

            throw new PiQResourceLockedException(
                string.Join(", ", resourceIdList),
                "Failed to acquire resources",
                ex);
        }
    }

    /// <summary>
    /// Disposes the resource manager
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        // Clean up all scopes
        await CleanupAllScopesAsync().ConfigureAwait(false);

        // Dispose all locks
        foreach (var resourceLock in _resourceLocks.Values)
        {
            await resourceLock.DisposeAsync().ConfigureAwait(false);
        }

        // Dispose the manager lock
        await _managerLock.DisposeAsync().ConfigureAwait(false);

        // Clear collections
        _resourceScopes.Clear();
        _typedResources.Clear();
        _resourceLocks.Clear();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Checks if the resource manager is disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the resource manager is disposed</exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }
}
