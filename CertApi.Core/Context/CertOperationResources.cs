// CertApi.Core/Context/CertOperationResources.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Core;
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Context;

/// <summary>
/// Implementation of IOperationResources
/// </summary>
public class CertOperationResources : ICertOperationResources
{
    private readonly ICertResourceContext _resourceContext;
    private readonly ILogger<CertOperationResources> _logger;

    #region LoggerMessage Delegates

    private static readonly Action<ILogger, string, string, Exception?> LogGettingResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(GetResourceAsync)),
            "Getting resource {ResourceId} of type {ResourceType}");

    private static readonly Action<ILogger, string, string, Exception> LogFailedToGetResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(2, nameof(GetResourceAsync)),
            "Failed to get resource {ResourceId} of type {ResourceType}");

    private static readonly Action<ILogger, string, string, Exception?> LogTrackingResource =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(3, nameof(TrackResourceAsync)),
            "Tracking resource {ResourceId} of type {ResourceType}");

    private static readonly Action<ILogger, string, TimeSpan, Exception?> LogAttemptingToAcquireResource =
        LoggerMessage.Define<string, TimeSpan>(
            LogLevel.Debug,
            new EventId(4, nameof(TryAcquireResourceAsync)),
            "Attempting to acquire resource {ResourceId} with timeout {Timeout}");

    private static readonly Action<ILogger, string, Exception?> LogReleasingResource =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(5, nameof(ReleaseResourceAsync)),
            "Releasing resource {ResourceId}");

    private static readonly Action<ILogger, Exception?> LogCreatingResourceScope =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(6, nameof(CreateResourceScopeAsync)),
            "Creating resource scope");

    #endregion

    /// <summary>
    /// Initializes a new instance of the CertOperationResources class
    /// </summary>
    /// <param name="resourceContext">Resource context</param>
    /// <param name="logger">Logger</param>
    public CertOperationResources(ICertResourceContext resourceContext, ILogger<CertOperationResources> logger)
    {
        _resourceContext = resourceContext ?? throw new ArgumentNullException(nameof(resourceContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a resource by ID (wrapper around non-async GetResource)
    /// </summary>
    /// <typeparam name="T">Resource type</typeparam>
    /// <param name="resourceId">Resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The requested resource</returns>
    public Task<T> GetResourceAsync<T>(string resourceId, CancellationToken cancellationToken = default)
        where T : class, ICertResource
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));

        LogGettingResource(_logger, resourceId, typeof(T).Name, null);

        try
        {
            // Note: GetResource is non-async in ICertResourceContext
            // We're wrapping it in a Task to maintain the async interface
            var resource = _resourceContext.GetResource<T>(resourceId);
            return Task.FromResult(resource);
        }
        catch (Exception ex)
        {
            LogFailedToGetResource(_logger, resourceId, typeof(T).Name, ex);
            throw;
        }
    }

    /// <summary>
    /// Tracks a resource
    /// </summary>
    /// <typeparam name="T">Resource type</typeparam>
    /// <param name="resource">Resource to track</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tracked resource</returns>
    public async Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken = default)
        where T : class, ICertResource
    {
        ArgumentNullException.ThrowIfNull(resource, nameof(resource));

        LogTrackingResource(_logger, resource.ResourceId, typeof(T).Name, null);

        return await _resourceContext.TrackResourceAsync(resource, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Attempts to acquire a resource lock
    /// </summary>
    /// <param name="resourceId">Resource ID</param>
    /// <param name="timeout">Maximum time to wait</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the resource was acquired; otherwise, false</returns>
    public async Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));

        LogAttemptingToAcquireResource(_logger, resourceId, timeout, null);

        return await _resourceContext.TryAcquireResourceAsync(resourceId, timeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Releases a resource lock
    /// </summary>
    /// <param name="resourceId">Resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));

        LogReleasingResource(_logger, resourceId, null);

        await _resourceContext.ReleaseResourceAsync(resourceId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a resource scope
    /// </summary>
    /// <param name="resourceIds">Resource IDs to include in the scope</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Resource scope</returns>
    public async Task<ICertResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resourceIds, nameof(resourceIds));

        LogCreatingResourceScope(_logger, null);

        return await _resourceContext.CreateResourceScopeAsync(resourceIds, cancellationToken).ConfigureAwait(false);
    }
}
