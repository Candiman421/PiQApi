// PiQApi.Core/Authentication/TokenCache/CertMemoryTokenCache.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Monitoring;
using PiQApi.Core.Authentication.TokenCache.Interfaces;
using PiQApi.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace PiQApi.Core.Authentication.TokenCache;

/// <summary>
/// Memory-based implementation of token cache
/// </summary>
public class CertMemoryTokenCache : ICertTokenCache
{
    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, string, Exception?> _tokenExpiredLog =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(GetTokenAsync)),
            "[{CorrelationId}] Token for key {CacheKey} is expired");

    private static readonly Action<ILogger, string, string, Exception?> _tokenFoundLog =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(2, nameof(GetTokenAsync)),
            "[{CorrelationId}] Token found in cache for key {CacheKey}");

    private static readonly Action<ILogger, string, string, Exception?> _tokenNotFoundLog =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(3, nameof(GetTokenAsync)),
            "[{CorrelationId}] Token not found in cache for key {CacheKey}");

    private static readonly Action<ILogger, string, string, DateTime, Exception?> _tokenCachedLog =
        LoggerMessage.Define<string, string, DateTime>(
            LogLevel.Debug,
            new EventId(4, nameof(SetTokenAsync)),
            "[{CorrelationId}] Token cached for key {CacheKey}, expires on {ExpiresOn}");

    private static readonly Action<ILogger, string, Exception?> _cacheSizeExceededLog =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(5, nameof(SetTokenAsync)),
            "[{CorrelationId}] Cache size exceeds limit, cleaning up expired items");

    private static readonly Action<ILogger, string, string, Exception?> _tokenRemovedLog =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(6, nameof(RemoveTokenAsync)),
            "[{CorrelationId}] Token removed from cache for key {CacheKey}");

    private static readonly Action<ILogger, string, Exception?> _cacheClearedLog =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(7, nameof(ClearAsync)),
            "[{CorrelationId}] Token cache cleared");

    private static readonly Action<ILogger, string, string, Exception?> _expiredTokenRemovedLog =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(8, nameof(CleanupExpiredTokens)),
            "[{CorrelationId}] Expired token removed for key {CacheKey}");

    private static readonly Action<ILogger, string, int, Exception?> _expiredTokensCountLog =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(9, nameof(CleanupExpiredTokens)),
            "[{CorrelationId}] Removed {Count} expired tokens from cache");

    private readonly ILogger<CertMemoryTokenCache> _logger;
    private readonly ICertCacheMetrics _cacheMetrics;
    private readonly ConcurrentDictionary<string, CertTokenInfo> _cache;
    private readonly CertCacheOptions _options;
    private readonly ICertCorrelationContext _correlationContext;
    private readonly ICertAsyncLock _cacheLock;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertMemoryTokenCache"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="options">Cache options</param>
    /// <param name="cacheMetrics">Metrics tracker for cache operations</param>
    /// <param name="correlationContext">Correlation context for tracking operations</param>
    /// <param name="asyncLockFactory">Async lock factory for thread-safe operations</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public CertMemoryTokenCache(
        ILogger<CertMemoryTokenCache> logger,
        IOptions<CertCacheOptions> options,
        ICertCacheMetrics cacheMetrics,
        ICertCorrelationContext correlationContext,
        ICertAsyncLockFactory asyncLockFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _cacheMetrics = cacheMetrics ?? throw new ArgumentNullException(nameof(cacheMetrics));
        _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        ArgumentNullException.ThrowIfNull(asyncLockFactory);

        _cache = new ConcurrentDictionary<string, CertTokenInfo>(StringComparer.OrdinalIgnoreCase);
        _cacheLock = asyncLockFactory.Create();
    }

    /// <summary>
    /// Gets a token from the cache
    /// </summary>
    public async Task<CertTokenInfo?> GetTokenAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentException.ThrowIfNullOrEmpty(cacheKey);

        // Track operation in correlation context
        _correlationContext.AddProperty("CacheOperation", "GetToken");
        _correlationContext.AddProperty("CacheKey", cacheKey);

        if (_cache.TryGetValue(cacheKey, out var token))
        {
            if (token.IsExpired)
            {
                _tokenExpiredLog(_logger, _correlationContext.CorrelationId, cacheKey, null);
                await RemoveTokenAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                _cacheMetrics.RecordCacheActivity(false);
                return null;
            }

            _tokenFoundLog(_logger, _correlationContext.CorrelationId, cacheKey, null);
            _cacheMetrics.RecordCacheActivity(true);
            return token;
        }

        _tokenNotFoundLog(_logger, _correlationContext.CorrelationId, cacheKey, null);
        _cacheMetrics.RecordCacheActivity(false);
        return null;
    }

    /// <summary>
    /// Sets a token in the cache
    /// </summary>
    public async Task SetTokenAsync(string cacheKey, CertTokenInfo token, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentException.ThrowIfNullOrEmpty(cacheKey);
        ArgumentNullException.ThrowIfNull(token);

        // Track operation in correlation context
        _correlationContext.AddProperty("CacheOperation", "SetToken");
        _correlationContext.AddProperty("CacheKey", cacheKey);

        _cache[cacheKey] = token;
        _tokenCachedLog(_logger, _correlationContext.CorrelationId, cacheKey, token.ExpiresOn, null);

        // Clean up if cache is too large - use async lock to ensure cleanup is not concurrent
        if (_cache.Count > _options.MaxItems)
        {
            using var lockReleaser = await _cacheLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

            // Double-check the condition after acquiring the lock
            if (_cache.Count > _options.MaxItems)
            {
                _cacheSizeExceededLog(_logger, _correlationContext.CorrelationId, null);
                await CleanupExpiredTokensInternalAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Removes a token from the cache
    /// </summary>
    public Task RemoveTokenAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentException.ThrowIfNullOrEmpty(cacheKey);

        // Track operation in correlation context
        _correlationContext.AddProperty("CacheOperation", "RemoveToken");
        _correlationContext.AddProperty("CacheKey", cacheKey);

        if (_cache.TryRemove(cacheKey, out _))
        {
            _tokenRemovedLog(_logger, _correlationContext.CorrelationId, cacheKey, null);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears all tokens from the cache
    /// </summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        // Track operation in correlation context
        _correlationContext.AddProperty("CacheOperation", "ClearCache");

        // Use async lock to ensure thread safety for clear operation
        using var lockReleaser = await _cacheLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        _cache.Clear();
        _cacheClearedLog(_logger, _correlationContext.CorrelationId, null);
    }

    /// <summary>
    /// Gets the count of tokens in the cache
    /// </summary>
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        // Track operation in correlation context
        _correlationContext.AddProperty("CacheOperation", "GetCount");
        _correlationContext.AddProperty("CacheSize", _cache.Count);

        return Task.FromResult(_cache.Count);
    }

    /// <summary>
    /// Cleans up expired tokens from the cache
    /// </summary>
    public async Task CleanupExpiredTokens(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        using var lockReleaser = await _cacheLock.AcquireAsync(cancellationToken).ConfigureAwait(false);
        await CleanupExpiredTokensInternalAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Cleans up expired tokens from the cache (internal implementation)
    /// </summary>
    /// <remarks>
    /// Caller must hold the cache lock before calling this method
    /// </remarks>
    private async Task CleanupExpiredTokensInternalAsync(CancellationToken cancellationToken = default)
    {
        // Track cleanup operation
        _correlationContext.AddProperty("CacheOperation", "CleanupExpiredTokens");

        var expiredKeys = _cache.Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        _correlationContext.AddProperty("ExpiredTokensCount", expiredKeys.Count);

        foreach (var key in expiredKeys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_cache.TryRemove(key, out _))
            {
                _expiredTokenRemovedLog(_logger, _correlationContext.CorrelationId, key, null);
            }
        }

        _expiredTokensCountLog(_logger, _correlationContext.CorrelationId, expiredKeys.Count, null);

        // If we still have too many items after removing expired ones, 
        // remove the oldest tokens until we're within the limit
        if (_cache.Count > _options.MaxItems)
        {
            var oldestTokens = _cache
                .OrderBy(kvp => kvp.Value.ExpiresOn)
                .Take(_cache.Count - _options.MaxItems + 10) // Remove extra to avoid immediate cleanup
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in oldestTokens)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_cache.TryRemove(key, out _))
                {
                    _correlationContext.AddProperty("RemovalReason", "CacheSizeLimit");
                    _tokenRemovedLog(_logger, _correlationContext.CorrelationId, key, null);
                }
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the cache
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await _cacheLock.DisposeAsync().ConfigureAwait(false);
    }
}