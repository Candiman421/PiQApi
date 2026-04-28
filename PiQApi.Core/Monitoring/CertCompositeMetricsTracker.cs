// PiQApi.Core/Monitoring/CertCompositeMetricsTracker.cs
using System.Collections.Concurrent;
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Monitoring;
using PiQApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// Implementation of ICertCompositeMetricsTracker that combines multiple metrics trackers
/// </summary>
public class CertCompositeMetricsTracker : ICertCompositeMetricsTracker
{
    private readonly ILogger<CertCompositeMetricsTracker> _logger;
    private readonly ConcurrentBag<ICertOperationMetrics> _trackers = new();
    private readonly ICertTimeProvider _timeProvider;
    private readonly TimeSpan _operationStartTime;
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, TimeSpan> _startTimes = new();
    private readonly ConcurrentDictionary<string, TimeSpan> _elapsedTimes = new();
    private readonly ConcurrentDictionary<string, double> _customMetrics = new();

    // Connection metrics fields
    private int _totalConnections;
    private int _activeConnections;
    private int _failedConnections;
    private DateTime _lastFailureTime = DateTime.MinValue;

    // Cache metrics fields
    private int _cacheHits;
    private int _cacheMisses;

    // Logging delegates (keeping existing high-performance logging)
    private static readonly Action<ILogger, string, ICertOperationMetrics, Exception?> LogTrackerAdded =
        LoggerMessage.Define<string, ICertOperationMetrics>(
            LogLevel.Debug,
            new EventId(1, "AddTracker"),
            "Added metrics tracker with ID {CorrelationId}: {Tracker}");

    private static readonly Action<ILogger, string, long, long, Exception?> LogIncrementCounter =
        LoggerMessage.Define<string, long, long>(
            LogLevel.Trace,
            new EventId(2, "IncrementCounter"),
            "Incremented counter {CounterName} by {Increment} to {NewValue}");

    private static readonly Action<ILogger, bool, Exception?> LogCacheActivity =
        LoggerMessage.Define<bool>(
            LogLevel.Trace,
            new EventId(3, "RecordCacheActivity"),
            "Recorded cache {IsHit}");

    private static readonly Action<ILogger, TimeSpan, Exception?> LogConnectionAcquired =
        LoggerMessage.Define<TimeSpan>(
            LogLevel.Trace,
            new EventId(4, "RecordConnectionAcquired"),
            "Recorded connection acquired in {AcquisitionTime}");

    private static readonly Action<ILogger, TimeSpan, Exception?> LogConnectionReleased =
        LoggerMessage.Define<TimeSpan>(
            LogLevel.Trace,
            new EventId(5, "RecordConnectionReleased"),
            "Recorded connection released with lifetime {Lifetime}");

    private static readonly Action<ILogger, string, Exception?> LogConnectionFailure =
        LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(6, "RecordConnectionFailure"),
            "Recorded connection failure with error code {ErrorCode}");

    private static readonly Action<ILogger, Exception?> LogResetCounters =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(7, "ResetCounters"),
            "Reset all metrics counters");

    private static readonly Action<ILogger, int, int, Exception?> LogCreateSnapshot =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(8, "CreateSnapshot"),
            "Created metrics snapshot with {CounterCount} counters and {TimerCount} timers");

    /// <summary>
    /// Creates a new composite metrics tracker
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public CertCompositeMetricsTracker(
        ILogger<CertCompositeMetricsTracker> logger,
        ICertTimeProvider timeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _operationStartTime = _timeProvider.GetTimestamp();
    }

    #region ICertConnectionMetrics Implementation

    /// <summary>
    /// Gets the total number of connections ever acquired
    /// </summary>
    public int TotalConnections => Interlocked.CompareExchange(ref _totalConnections, 0, 0);

    /// <summary>
    /// Gets the number of currently active connections
    /// </summary>
    public int ActiveConnections => Interlocked.CompareExchange(ref _activeConnections, 0, 0);

    /// <summary>
    /// Gets the number of connections that have failed
    /// </summary>
    public int FailedConnections => Interlocked.CompareExchange(ref _failedConnections, 0, 0);

    /// <summary>
    /// Gets the timestamp of the last connection failure
    /// </summary>
    public DateTime LastFailureTime => _lastFailureTime;

    /// <summary>
    /// Records a new connection acquisition
    /// </summary>
    /// <param name="acquisitionTime">Time taken to acquire the connection</param>
    public void RecordConnectionAcquired(TimeSpan acquisitionTime)
    {
        Interlocked.Increment(ref _totalConnections);
        Interlocked.Increment(ref _activeConnections);

        LogConnectionAcquired(_logger, acquisitionTime, null);

        // Record in operational metrics as well
        RecordTime("ConnectionAcquisition", acquisitionTime);
        IncrementCounter("ConnectionsAcquired");
    }

    /// <summary>
    /// Records a connection being released
    /// </summary>
    /// <param name="lifetime">Total lifetime of the connection</param>
    public void RecordConnectionReleased(TimeSpan lifetime)
    {
        int currentActive, newActive;
        do
        {
            currentActive = Interlocked.CompareExchange(ref _activeConnections, 0, 0);
            if (currentActive <= 0)
            {
                newActive = 0;
                break;
            }

            newActive = currentActive - 1;
        } while (Interlocked.CompareExchange(ref _activeConnections, newActive, currentActive) != currentActive);

        LogConnectionReleased(_logger, lifetime, null);

        // Record in operational metrics as well
        RecordTime("ConnectionLifetime", lifetime);
        IncrementCounter("ConnectionsReleased");
    }

    /// <summary>
    /// Records a connection failure
    /// </summary>
    /// <param name="errorCode">Unique error code for the connection failure</param>
    public void RecordConnectionFailure(string errorCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        // Decrement active connections
        int currentActive, newActive;
        do
        {
            currentActive = Interlocked.CompareExchange(ref _activeConnections, 0, 0);
            if (currentActive <= 0)
            {
                newActive = 0;
                break;
            }

            newActive = currentActive - 1;
        } while (Interlocked.CompareExchange(ref _activeConnections, newActive, currentActive) != currentActive);

        // Increment failed connections
        Interlocked.Increment(ref _failedConnections);

        // Record failure time
        _lastFailureTime = _timeProvider.UtcNow;

        LogConnectionFailure(_logger, errorCode, null);

        // Record in operational metrics as well
        IncrementCounter("ConnectionFailures");
        IncrementCounter($"ConnectionFailure_{errorCode}");
    }

    #endregion

    #region ICertCacheMetrics Implementation

    /// <summary>
    /// Gets the number of cache hits
    /// </summary>
    public int CacheHits => Interlocked.CompareExchange(ref _cacheHits, 0, 0);

    /// <summary>
    /// Gets the number of cache misses
    /// </summary>
    public int CacheMisses => Interlocked.CompareExchange(ref _cacheMisses, 0, 0);

    /// <summary>
    /// Gets the cache hit rate as a percentage
    /// </summary>
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

    /// <summary>
    /// Records a cache access event
    /// </summary>
    /// <param name="isHit">True if the cache was hit, false if missed</param>
    public void RecordCacheActivity(bool isHit)
    {
        if (isHit)
        {
            Interlocked.Increment(ref _cacheHits);
            IncrementCounter("CacheHits");
        }
        else
        {
            Interlocked.Increment(ref _cacheMisses);
            IncrementCounter("CacheMisses");
        }

        LogCacheActivity(_logger, isHit, null);
    }

    #endregion

    #region ICertOperationMetrics Implementation

    /// <summary>
    /// Gets the total duration of all operations
    /// </summary>
    public TimeSpan Duration => _timeProvider.GetElapsedTime(_operationStartTime);

    /// <summary>
    /// Gets the collection of counters
    /// </summary>
    public IReadOnlyDictionary<string, long> Counters => _counters;

    /// <summary>
    /// Gets the collection of timers
    /// </summary>
    public IReadOnlyDictionary<string, TimeSpan> Timers
    {
        get
        {
            var result = new Dictionary<string, TimeSpan>(_elapsedTimes);

            foreach (var timer in _startTimes)
            {
                result[timer.Key] = _timeProvider.GetElapsedTime(timer.Value);
            }

            return result;
        }
    }

    /// <summary>
    /// Gets the collection of custom metrics
    /// </summary>
    public IReadOnlyDictionary<string, double> CustomMetrics => _customMetrics;

    /// <summary>
    /// Gets the count of successful operations
    /// </summary>
    public long SuccessfulOperations => GetCounter("SuccessfulOperations");

    /// <summary>
    /// Gets the count of failed operations
    /// </summary>
    public long FailedOperations => GetCounter("FailedOperations");

    /// <summary>
    /// Gets the total count of operations (successful + failed)
    /// </summary>
    public long OperationCount => SuccessfulOperations + FailedOperations;

    /// <summary>
    /// Gets the success rate percentage (successful operations / total operations)
    /// </summary>
    public double SuccessRate => OperationCount > 0 ? (double)SuccessfulOperations / OperationCount * 100 : 0;

    /// <summary>
    /// Gets the average operation duration in milliseconds
    /// </summary>
    public long AverageOperationDurationMs => OperationCount > 0 ? (long)(GetTimerElapsed("OperationDuration").TotalMilliseconds / OperationCount) : 0;

    /// <summary>
    /// Increments a counter by the specified amount
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <param name="increment">The amount to increment by</param>
    /// <returns>The new counter value</returns>
    public long IncrementCounter(string name, long increment = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var newValue = _counters.AddOrUpdate(name, increment, (_, value) => value + increment);

        // Forward to all trackers
        foreach (var tracker in _trackers)
        {
            tracker.IncrementCounter(name, increment);
        }

        LogIncrementCounter(_logger, name, increment, newValue, null);

        return newValue;
    }

    /// <summary>
    /// Starts a timer with the specified name
    /// </summary>
    /// <param name="name">Timer name</param>
    public void StartTimer(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _startTimes[name] = _timeProvider.GetTimestamp();

        // Forward to all trackers
        foreach (var tracker in _trackers)
        {
            tracker.StartTimer(name);
        }
    }

    /// <summary>
    /// Stops a timer with the specified name
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <returns>The elapsed time</returns>
    public TimeSpan StopTimer(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (_startTimes.TryRemove(name, out var startTime))
        {
            var elapsed = _timeProvider.GetElapsedTime(startTime);
            _elapsedTimes[name] = elapsed;

            // Forward to all trackers
            foreach (var tracker in _trackers)
            {
                tracker.StopTimer(name);
            }

            return elapsed;
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Records the time taken by an operation
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <param name="elapsedTime">The elapsed time</param>
    public void RecordTime(string name, TimeSpan elapsedTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _elapsedTimes[name] = elapsedTime;

        // Forward to all trackers
        foreach (var tracker in _trackers)
        {
            tracker.RecordTime(name, elapsedTime);
        }
    }

    /// <summary>
    /// Records an operation with its duration and success status
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="duration">Duration of the operation</param>
    /// <param name="success">Whether the operation was successful</param>
    public void RecordOperation(string operationName, TimeSpan duration, bool success)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        // Record time and success/failure counters
        RecordTime(operationName, duration);
        IncrementCounter(success ? "SuccessfulOperations" : "FailedOperations");
        IncrementCounter($"{operationName}_{(success ? "Success" : "Failure")}");

        // Forward to all trackers
        foreach (var tracker in _trackers)
        {
            tracker.RecordOperation(operationName, duration, success);
        }
    }

    /// <summary>
    /// Gets the value of a counter
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <returns>The counter value, or 0 if the counter doesn't exist</returns>
    public long GetCounter(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return _counters.TryGetValue(name, out var value) ? value : 0;
    }

    /// <summary>
    /// Gets the elapsed time of a timer
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <returns>The elapsed time, or TimeSpan.Zero if the timer doesn't exist</returns>
    public TimeSpan GetTimerElapsed(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (_elapsedTimes.TryGetValue(name, out var elapsed))
        {
            return elapsed;
        }

        if (_startTimes.TryGetValue(name, out var startTime))
        {
            return _timeProvider.GetElapsedTime(startTime);
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Creates a snapshot of the current metrics
    /// </summary>
    /// <returns>A dictionary containing all metrics</returns>
    public IDictionary<string, object> CreateSnapshot()
    {
        var snapshot = new Dictionary<string, object>
        {
            // Connection metrics
            ["TotalConnections"] = TotalConnections,
            ["ActiveConnections"] = ActiveConnections,
            ["FailedConnections"] = FailedConnections,
            ["LastFailureTime"] = LastFailureTime,

            // Cache metrics
            ["CacheHits"] = CacheHits,
            ["CacheMisses"] = CacheMisses,
            ["CacheHitRate"] = CacheHitRate,

            // Operation metrics
            ["Duration"] = Duration.TotalMilliseconds,
            ["SuccessfulOperations"] = SuccessfulOperations,
            ["FailedOperations"] = FailedOperations,
            ["OperationCount"] = OperationCount,
            ["SuccessRate"] = SuccessRate,
            ["AverageOperationDurationMs"] = AverageOperationDurationMs
        };

        // Add all counters
        foreach (var counter in Counters)
        {
            snapshot[$"Counter:{counter.Key}"] = counter.Value;
        }

        // Add all timers
        foreach (var timer in Timers)
        {
            snapshot[$"Timer:{timer.Key}"] = timer.Value.TotalMilliseconds;
        }

        // Add all custom metrics
        foreach (var metric in CustomMetrics)
        {
            snapshot[$"CustomMetric:{metric.Key}"] = metric.Value;
        }

        // Combine snapshots from all trackers
        foreach (var tracker in _trackers)
        {
            var trackerSnapshot = tracker.CreateSnapshot();
            foreach (var item in trackerSnapshot)
            {
                snapshot[$"Tracker:{item.Key}"] = item.Value;
            }
        }

        LogCreateSnapshot(_logger, Counters.Count, Timers.Count, null);
        return snapshot;
    }

    #endregion

    #region ICertMetricsProvider Implementation

    /// <summary>
    /// Gets a snapshot of the current metrics asynchronously
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task containing the metrics snapshot</returns>
    public async Task<ICertMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // Check for cancellation
        cancellationToken.ThrowIfCancellationRequested();

        // Create standard snapshot with metrics dictionary
        var metricsDict = CreateSnapshot();

        // Create a new instance with populated values
        var snapshot = new CertMetricsSnapshot(metricsDict)
        {
            SuccessfulOperations = SuccessfulOperations,
            FailedOperations = FailedOperations,
            TotalDurationMs = (long)Duration.TotalMilliseconds,
            OperationCount = OperationCount,
            SuccessRate = SuccessRate,
            AverageOperationDurationMs = AverageOperationDurationMs
        };

        return await Task.FromResult<ICertMetricsSnapshot>(snapshot).ConfigureAwait(false);
    }

    /// <summary>
    /// Resets all tracked metrics to their initial state
    /// </summary>
    public void ResetCounters()
    {
        // Reset connection metrics
        Interlocked.Exchange(ref _totalConnections, 0);
        Interlocked.Exchange(ref _activeConnections, 0);
        Interlocked.Exchange(ref _failedConnections, 0);
        _lastFailureTime = DateTime.MinValue;

        // Reset cache metrics
        Interlocked.Exchange(ref _cacheHits, 0);
        Interlocked.Exchange(ref _cacheMisses, 0);

        // Reset operational metrics
        _counters.Clear();
        _startTimes.Clear();
        _elapsedTimes.Clear();
        _customMetrics.Clear();

        // Reset contained trackers
        foreach (var tracker in _trackers)
        {
            if (tracker is ICertMetricsProvider metricsProvider)
            {
                metricsProvider.ResetCounters();
            }
        }

        LogResetCounters(_logger, null);
    }

    #endregion

    #region ICertCompositeMetricsTracker Implementation

    /// <summary>
    /// Records a custom metric value
    /// </summary>
    /// <param name="name">Name of the metric</param>
    /// <param name="value">Metric value</param>
    public void RecordCustomMetric(string name, double value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _customMetrics[name] = value;

        // Forward to all trackers
        foreach (var tracker in _trackers)
        {
            tracker.RecordCustomMetric(name, value);
        }
    }

    /// <summary>
    /// Adds a metrics tracker to the composite
    /// </summary>
    /// <param name="tracker">Metrics tracker to add</param>
    public void AddTracker(ICertOperationMetrics tracker)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        _trackers.Add(tracker);
        LogTrackerAdded(_logger, Guid.NewGuid().ToString(), tracker, null);
    }

    #endregion
}