// PiQApi.Core/Threading/PiQAsyncResourceScope.cs
using Microsoft.Extensions.Logging;
using PiQApi.Core.Core;
using PiQApi.Abstractions.Core;

namespace PiQApi.Core.Threading;

/// <summary>
/// Provides a scope for managing asynchronous resources
/// </summary>
public sealed class PiQAsyncResourceScope : IAsyncDisposable, IPiQResourceScope, IDisposable
{
    private readonly List<IDisposable> _resources;
    private readonly PiQResourceContext? _context;
    private readonly IReadOnlyCollection<string>? _resourceIds;
    private readonly ILogger? _logger;
    private bool _isDisposed;

    #region LoggerMessage Delegates
    private static readonly Action<ILogger, string, Exception?> _logCreatedForSemaphore =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, "CreateAsync"),
            "Created resource scope for semaphore {SemaphoreId}");

    private static readonly Action<ILogger, Exception?> _logCreatedMultipleResources =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(2, "Create"),
            "Creating resource scope with multiple resources");

    private static readonly Action<ILogger, int, Exception?> _logDisposing =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(3, "DisposeAsync"),
            "Disposing resource scope with {ResourceCount} resources");

    private static readonly Action<ILogger, Exception> _logDisposalError =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(4, "DisposalError"),
            "Error disposing resource in scope");

    private static readonly Action<ILogger, string, Exception?> _logReleasingResource =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(5, "ReleaseResource"),
            "Releasing resource {ResourceId}");

    private static readonly Action<ILogger, string, Exception?> _logAddingResource =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(6, "AddResource"),
            "Adding resource of type {ResourceType}");

    private static readonly Action<ILogger, string, Exception?> _logRemovingResource =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(7, "RemoveResource"),
            "Removing resource of type {ResourceType}");
    #endregion

    /// <summary>
    /// Gets the ID of this resource scope
    /// </summary>
    public string ScopeId { get; }

    /// <summary>
    /// Gets the creation timestamp for this scope
    /// </summary>
    public DateTimeOffset Created { get; }

    /// <summary>
    /// Gets the collection of resource IDs in this scope
    /// </summary>
    public IReadOnlyCollection<string> ResourceIds => _resourceIds ?? Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of the PiQAsyncResourceScope class for direct resource management
    /// </summary>
    /// <param name="resources">The resources to include in the scope</param>
    /// <param name="logger">Optional logger instance</param>
    private PiQAsyncResourceScope(IEnumerable<IDisposable> resources, ILogger? logger = null)
    {
        _resources = new List<IDisposable>(resources);
        _logger = logger;
        _context = null;
        _resourceIds = null;
        ScopeId = Guid.NewGuid().ToString();
        Created = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the PiQAsyncResourceScope class for resource context
    /// </summary>
    /// <param name="context">The resource context</param>
    /// <param name="resourceIds">The resource IDs to manage</param>
    /// <param name="logger">The logger</param>
    public PiQAsyncResourceScope(PiQResourceContext context, IEnumerable<string> resourceIds, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _resourceIds = resourceIds?.ToList() ?? throw new ArgumentNullException(nameof(resourceIds));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _resources = new List<IDisposable>();
        ScopeId = Guid.NewGuid().ToString();
        Created = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a new resource scope with the specified semaphore
    /// </summary>
    /// <param name="semaphore">The semaphore to include in the scope</param>
    /// <param name="logger">Optional logger instance</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A resource scope</returns>
    public static async Task<PiQAsyncResourceScope> CreateAsync(
        PiQAsyncSemaphore semaphore,
        ILogger<PiQAsyncResourceScope>? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(semaphore);

        var releaser = await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (logger != null)
        {
            _logCreatedForSemaphore(logger, semaphore.Id, null);
        }
        return new PiQAsyncResourceScope(new[] { releaser }, logger);
    }

    /// <summary>
    /// Creates a new resource scope with the specified resources
    /// </summary>
    /// <param name="resources">The resources to include in the scope</param>
    /// <param name="logger">Optional logger instance</param>
    /// <returns>A resource scope</returns>
    public static PiQAsyncResourceScope Create(
        IEnumerable<IDisposable> resources,
        ILogger<PiQAsyncResourceScope>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(resources);

        if (logger != null)
        {
            _logCreatedMultipleResources(logger, null);
        }
        return new PiQAsyncResourceScope(resources, logger);
    }

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

        if (_logger != null)
        {
            _logAddingResource(_logger, typeof(T).Name, null);
        }

        // If the resource context is available and resource implements IPiQResource, track it
        if (_context != null && resource is IPiQResource certResource)
        {
            await _context.TrackResourceAsync(certResource, cancellationToken).ConfigureAwait(false);
        }
        // If the resource is disposable, add it to our resources collection
        else if (resource is IDisposable disposable)
        {
            _resources.Add(disposable);
        }

        return resource;
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

        if (_logger != null)
        {
            _logRemovingResource(_logger, typeof(T).Name, null);
        }

        // If the resource context is available and resource implements IPiQResource, handle it there
        if (_context != null && resource is IPiQResource certResource)
        {
            await _context.ReleaseResourceAsync(certResource.ResourceId, cancellationToken).ConfigureAwait(false);
        }
        // If the resource is disposable, remove it from our collection
        else if (resource is IDisposable disposable)
        {
            _resources.Remove(disposable);
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Cleans up resources in the scope
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        // Release resource context resources if available
        if (_context != null && _resourceIds != null)
        {
            foreach (var id in _resourceIds.Reverse())
            {
                try
                {
                    if (_logger != null)
                    {
                        _logReleasingResource(_logger, id, null);
                    }
                    await _context.ReleaseResourceAsync(id, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logDisposalError(_logger, ex);
                    }
                }
            }
        }

        // Dispose direct resources
        if (_resources.Count > 0 && _logger != null)
        {
            _logDisposing(_logger, _resources.Count, null);
        }

        foreach (var resource in _resources)
        {
            try
            {
                resource.Dispose();
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logDisposalError(_logger, ex);
                }
            }
        }
    }

    /// <summary>
    /// Disposes the resource scope, releasing all resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await CleanupAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resource scope synchronously
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        // Release resource context resources if available
        if (_context != null && _resourceIds != null)
        {
            foreach (var id in _resourceIds.Reverse())
            {
                try
                {
                    _context.ReleaseResourceAsync(id).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logDisposalError(_logger, ex);
                    }
                }
            }
        }

        // Dispose direct resources
        foreach (var resource in _resources)
        {
            try
            {
                resource.Dispose();
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logDisposalError(_logger, ex);
                }
            }
        }

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }
}