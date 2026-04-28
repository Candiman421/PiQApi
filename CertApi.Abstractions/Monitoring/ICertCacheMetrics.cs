// CertApi.Abstractions/Monitoring/ICertCacheMetrics.cs
namespace CertApi.Abstractions.Monitoring;

/// <summary>
/// Provides metrics tracking for cache operations
/// </summary>
public interface ICertCacheMetrics : ICertMetricsProvider
{
    /// <summary>
    /// Gets the number of cache hits
    /// </summary>
    int CacheHits { get; }

    /// <summary>
    /// Gets the number of cache misses
    /// </summary>
    int CacheMisses { get; }

    /// <summary>
    /// Gets the cache hit rate as a percentage
    /// </summary>
    double CacheHitRate { get; }

    /// <summary>
    /// Records a cache access event
    /// </summary>
    /// <param name="isHit">True if the cache was hit, false if missed</param>
    void RecordCacheActivity(bool isHit);
}