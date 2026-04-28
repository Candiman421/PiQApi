// PiQApi.Core/Monitoring/PiQCacheMetricsTracker.cs
using PiQApi.Abstractions.Monitoring;
using PiQApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// Tracks cache metrics in a thread-safe manner
/// </summary>
public class PiQCacheMetricsTracker : IPiQCacheMetrics
{
    private readonly ILogger<PiQCacheMetricsTracker> _logger;
    private readonly IPiQTimeProvider _timeProvider;
    private int _cacheHits;
    private int _cacheMisses;

    // LoggerMessage delegates for high-performance logging
    private static readonly Action<ILogger, int, int, double, Exception?> LogMetricsSnapshot =
        LoggerMessage.Define<int, int, double>(
            LogLevel.Debug,
            new EventId(1, nameof(GetMetricsSnapshotAsync)),
            "Cache metrics snapshot: Hits={Hits}, Misses={Misses}, Hit Rate={HitRate:F2}%");

    private static readonly Action<ILogger, string, int, Exception?> LogCacheActivity =
        LoggerMessage.Define<string, int>(
            LogLevel.Trace,
            new EventId(2, nameof(RecordCacheActivity)),
            "Cache {Result}: Current count={Count}");

    /// <summary>
    /// Initializes a new instance of the PiQCacheMetricsTracker class
    /// </summary>
    /// <param name="logger">Logger for recording metrics events</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public PiQCacheMetricsTracker(
        ILogger<PiQCacheMetricsTracker> logger,
        IPiQTimeProvider timeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public int CacheHits => Interlocked.CompareExchange(ref _cacheHits, 0, 0);

    /// <inheritdoc />
    public int CacheMisses => Interlocked.CompareExchange(ref _cacheMisses, 0, 0);

    /// <inheritdoc />
    public double CacheHitRate
    {
        get
        {
            var hits = CacheHits;
            var misses = CacheMisses;
            var total = hits + misses;

            return total > 0 ? hits / (double)total * 100 : 0;
        }
    }

    /// <inheritdoc />
    public void RecordCacheActivity(bool isHit)
    {
        int newValue = isHit
            ? Interlocked.Increment(ref _cacheHits)
            : Interlocked.Increment(ref _cacheMisses);

        LogCacheActivity(_logger, isHit ? "hit" : "miss", newValue, null);
    }

    /// <inheritdoc />
    public void ResetCounters()
    {
        Interlocked.Exchange(ref _cacheHits, 0);
        Interlocked.Exchange(ref _cacheMisses, 0);
    }

    /// <inheritdoc />
    public async Task<IPiQMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // Check for cancellation
        cancellationToken.ThrowIfCancellationRequested();

        // Read counters atomically
        var hits = Interlocked.CompareExchange(ref _cacheHits, 0, 0);
        var misses = Interlocked.CompareExchange(ref _cacheMisses, 0, 0);

        // Calculate hit rate
        var hitRate = hits + misses > 0
            ? hits / (double)(hits + misses) * 100
            : 0;

        // Log the metrics snapshot
        LogMetricsSnapshot(_logger, hits, misses, hitRate, null);

        // Create and return the snapshot
        var snapshot = new PiQMetricsSnapshot(
            cacheHitRate: hitRate
        );

        return await Task.FromResult<IPiQMetricsSnapshot>(snapshot).ConfigureAwait(false);
    }
}