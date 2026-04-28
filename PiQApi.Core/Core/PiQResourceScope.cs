// PiQApi.Core/Core/PiQResourceScope.cs
using PiQApi.Abstractions.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace PiQApi.Core.Core;

/// <summary>
/// Implementation of a resource scope for managing resources
/// </summary>
public class PiQResourceScope : IPiQResourceScope
{
    private readonly IPiQResourceManager _resourceManager;
    private readonly ConcurrentDictionary<string, object> _resources = new();
    private readonly ILogger _logger;
    private bool _isDisposed;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, string, Exception?> LogAddingResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(AddResourceAsync)),
            "Adding resource of type {ResourceType} to scope {ScopeId}");

    private static readonly Action<ILogger, string, string, Exception?> LogRemovingResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(2, nameof(RemoveResourceAsync)),
            "Removing resource of type {ResourceType} from scope {ScopeId}");

    private static readonly Action<ILogger, string, Exception?> LogCleaningUp =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(3, nameof(CleanupAsync)),
            "Cleaning up scope {ScopeId}");

    private static readonly Action<ILogger, string, Exception?> LogDisposeError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(4, "DisposeError"),
            "Error disposing resource scope {ScopeId}");

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceScope"/> class
    /// </summary>
    /// <param name="resourceManager">Resource manager</param>
    /// <param name="scopeId">Scope identifier</param>
    /// <param name="logger">Logger</param>
    public PiQResourceScope(
        IPiQResourceManager resourceManager,
        string scopeId,
        ILogger logger)
    {
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        ScopeId = scopeId ?? throw new ArgumentNullException(nameof(scopeId));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Created = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier for this resource scope
    /// </summary>
    public string ScopeId { get; }

    /// <summary>
    /// Gets the creation timestamp for this scope
    /// </summary>
    public DateTimeOffset Created { get; }

    /// <summary>
    /// Adds a resource to the scope
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <param name="resource">Resource to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added resource</returns>
    public async Task<T> AddResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(resource);

        var resourceType = typeof(T);
        LogAddingResource(_logger, resourceType.Name, ScopeId, null);

        // Track the resource with the resource manager
        var trackedResource = await _resourceManager.TrackResourceAsync(resource, cancellationToken).ConfigureAwait(false);

        // Add to local collection
        string resourceKey = resource is IPiQResource certResource
            ? certResource.ResourceId
            : resource.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);

        _resources[resourceKey] = trackedResource;

        return trackedResource;
    }

    /// <summary>
    /// Removes a resource from the scope
    /// </summary>
    /// <typeparam name="T">Type of resource</typeparam>
    /// <param name="resource">Resource to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RemoveResourceAsync<T>(T resource, CancellationToken cancellationToken = default) where T : class
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(resource);

        var resourceType = typeof(T);
        LogRemovingResource(_logger, resourceType.Name, ScopeId, null);

        // Remove from the resource manager
        await _resourceManager.RemoveTrackedResourceAsync(resource, cancellationToken).ConfigureAwait(false);

        // Remove from local collection
        string resourceKey = resource is IPiQResource certResource
            ? certResource.ResourceId
            : resource.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);

        _resources.TryRemove(resourceKey, out _);
    }

    /// <summary>
    /// Disposes the scope and cleans up all resources
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            return;
        }

        LogCleaningUp(_logger, ScopeId, null);

        // Clean up all resources in this scope
        await _resourceManager.CleanupScopeAsync(ScopeId, cancellationToken).ConfigureAwait(false);

        // Clear local resources
        _resources.Clear();
    }

    /// <summary>
    /// Disposes the scope asynchronously
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        try
        {
            // Clean up all resources
            await CleanupAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogDisposeError(_logger, ScopeId, ex);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Checks if the scope is disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the scope is disposed</exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(PiQResourceScope));
    }
}