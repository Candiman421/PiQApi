// PiQApi.Core/Monitoring/PiQOperationMetricsTracker.cs
using System.Collections.Concurrent;
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Monitoring;
using PiQApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// Implementation of metrics tracking for operations
/// </summary>
public class PiQOperationMetricsTracker : IPiQOperationMetrics
{
    private readonly ILogger<PiQOperationMetricsTracker> _logger;
    private readonly IPiQTimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, long> _counters = new ConcurrentDictionary<string, long>();
    private readonly ConcurrentDictionary<string, TimeSpan> _startTimes = new ConcurrentDictionary<string, TimeSpan>();
    private readonly ConcurrentDictionary<string, TimeSpan> _completedTimes = new ConcurrentDictionary<string, TimeSpan>();
    private readonly ConcurrentDictionary<string, double> _customMetrics = new ConcurrentDictionary<string, double>();
    private readonly TimeSpan _startTime;

    private long _successfulOperations;
    private long _failedOperations;
    private long _totalDurationMs;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, bool, double, Exception?> LogOperationRecorded =
        LoggerMessage.Define<string, bool, double>(
            LogLevel.Debug,
            new EventId(1, nameof(RecordOperation)),
            "Operation {OperationName} recorded. Success: {Success}, Duration: {DurationMs}ms");

    private static readonly Action<ILogger, string, double, Exception?> LogCustomMetricRecorded =
        LoggerMessage.Define<string, double>(
            LogLevel.Debug,
            new EventId(2, nameof(RecordCustomMetric)),
            "Custom metric {MetricName} recorded with value {Value}");

    private static readonly Action<ILogger, Exception?> LogMetricsReset =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(ResetCounters)),
            "All metrics counters reset");

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQOperationMetricsTracker"/> class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public PiQOperationMetricsTracker(
        ILogger<PiQOperationMetricsTracker> logger,
        IPiQTimeProvider timeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _startTime = _timeProvider.GetTimestamp();
    }

    /// <summary>
    /// Gets the total duration of the operation
    /// </summary>
    public TimeSpan Duration => _timeProvider.GetElapsedTime(_startTime);

    /// <summary>
    /// Gets the number of successful operations
    /// </summary>
    public long SuccessfulOperations => Interlocked.Read(ref _successfulOperations);

    /// <summary>
    /// Gets the number of failed operations
    /// </summary>
    public long FailedOperations => Interlocked.Read(ref _failedOperations);

    /// <summary>
    /// Gets the total number of operations
    /// </summary>
    public long OperationCount => SuccessfulOperations + FailedOperations;

    /// <summary>
    /// Gets the success rate as a percentage
    /// </summary>
    public double SuccessRate => OperationCount > 0 ?
        (double)SuccessfulOperations / OperationCount * 100 : 0;

    /// <summary>
    /// Gets the average operation duration in milliseconds
    /// </summary>
    public long AverageOperationDurationMs => OperationCount > 0 ?
        Interlocked.Read(ref _totalDurationMs) / OperationCount : 0;

    /// <summary>
    /// Gets the custom metrics dictionary
    /// </summary>
    public IReadOnlyDictionary<string, double> CustomMetrics => _customMetrics;

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
            var result = new Dictionary<string, TimeSpan>(_completedTimes);

            foreach (var timer in _startTimes)
            {
                result[timer.Key] = _timeProvider.GetElapsedTime(timer.Value);
            }

            return result;
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

        long durationMs = (long)duration.TotalMilliseconds;

        if (success)
        {
            Interlocked.Increment(ref _successfulOperations);
        }
        else
        {
            Interlocked.Increment(ref _failedOperations);
        }

        Interlocked.Add(ref _totalDurationMs, durationMs);
        RecordTime(operationName, duration);

        LogOperationRecorded(_logger, operationName, success, durationMs, null);
    }

    /// <summary>
    /// Records a custom metric value
    /// </summary>
    /// <param name="metricName">Name of the metric</param>
    /// <param name="value">Metric value</param>
    public void RecordCustomMetric(string metricName, double value)
    {
        ArgumentException.ThrowIfNullOrEmpty(metricName);
        _customMetrics[metricName] = value;
        LogCustomMetricRecorded(_logger, metricName, value, null);
    }

    /// <summary>
    /// Increments a counter by the specified amount
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <param name="increment">The amount to increment by</param>
    /// <returns>The new counter value</returns>
    public long IncrementCounter(string name, long increment = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return _counters.AddOrUpdate(name, increment, (_, value) => value + increment);
    }

    /// <summary>
    /// Starts a timer with the specified name
    /// </summary>
    /// <param name="name">Timer name</param>
    public void StartTimer(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _startTimes[name] = _timeProvider.GetTimestamp();
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
            _completedTimes[name] = elapsed;
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
        _completedTimes[name] = elapsedTime;
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
        if (_completedTimes.TryGetValue(name, out var elapsed))
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
    /// Creates a metrics snapshot with the current values
    /// </summary>
    /// <returns>A dictionary containing all metrics</returns>
    public IDictionary<string, object> CreateSnapshot()
    {
        var snapshot = new Dictionary<string, object>
        {
            ["Duration"] = Duration.TotalMilliseconds,
            ["TimerCount"] = Timers.Count,
            ["CounterCount"] = Counters.Count,
            ["SuccessfulOperations"] = SuccessfulOperations,
            ["FailedOperations"] = FailedOperations,
            ["OperationCount"] = OperationCount,
            ["SuccessRate"] = SuccessRate,
            ["AverageOperationDurationMs"] = AverageOperationDurationMs
        };

        foreach (var counter in Counters)
        {
            snapshot[$"Counter_{counter.Key}"] = counter.Value;
        }

        foreach (var timer in Timers)
        {
            snapshot[$"Timer_{timer.Key}"] = timer.Value.TotalMilliseconds;
        }

        foreach (var metric in CustomMetrics)
        {
            snapshot[$"CustomMetric_{metric.Key}"] = metric.Value;
        }

        return snapshot;
    }

    /// <summary>
    /// Gets a snapshot of the current metrics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A metrics snapshot</returns>
    public async Task<IPiQMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // Check for cancellation
        cancellationToken.ThrowIfCancellationRequested();

        // Create snapshot with metrics dictionary
        var metricsDict = CreateSnapshot();

        // Create a new instance with the dictionary
        var snapshot = new PiQMetricsSnapshot(metricsDict)
        {
            SuccessfulOperations = SuccessfulOperations,
            FailedOperations = FailedOperations,
            TotalDurationMs = Interlocked.Read(ref _totalDurationMs),
            // Add the calculated properties that tests are looking for
            OperationCount = OperationCount,
            SuccessRate = SuccessRate,
            AverageOperationDurationMs = AverageOperationDurationMs
        };

        return await Task.FromResult<IPiQMetricsSnapshot>(snapshot).ConfigureAwait(false);
    }

    /// <summary>
    /// Resets the metrics counters
    /// </summary>
    public void ResetCounters()
    {
        _counters.Clear();
        _startTimes.Clear();
        _completedTimes.Clear();
        _customMetrics.Clear();
        Interlocked.Exchange(ref _successfulOperations, 0);
        Interlocked.Exchange(ref _failedOperations, 0);
        Interlocked.Exchange(ref _totalDurationMs, 0);

        LogMetricsReset(_logger, null);
    }
}
