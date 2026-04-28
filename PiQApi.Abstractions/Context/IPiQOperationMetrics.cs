// PiQApi.Abstractions/Context/IPiQOperationMetrics.cs
using PiQApi.Abstractions.Monitoring;

namespace PiQApi.Abstractions.Context;

/// <summary>
/// Provides comprehensive metrics tracking for operations
/// </summary>
public interface IPiQOperationMetrics : IPiQMetricsProvider
{
    /// <summary>
    /// Gets the total number of successful operations
    /// </summary>
    long SuccessfulOperations { get; }

    /// <summary>
    /// Gets the total number of failed operations
    /// </summary>
    long FailedOperations { get; }

    /// <summary>
    /// Gets the total number of operations
    /// </summary>
    long OperationCount { get; }

    /// <summary>
    /// Gets the success rate as a percentage
    /// </summary>
    double SuccessRate { get; }

    /// <summary>
    /// Gets the average operation duration in milliseconds
    /// </summary>
    long AverageOperationDurationMs { get; }

    /// <summary>
    /// Gets the total duration of all operations
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Gets the collection of custom metrics
    /// </summary>
    IReadOnlyDictionary<string, double> CustomMetrics { get; }

    /// <summary>
    /// Gets the collection of counters
    /// </summary>
    IReadOnlyDictionary<string, long> Counters { get; }

    /// <summary>
    /// Gets the collection of timers
    /// </summary>
    IReadOnlyDictionary<string, TimeSpan> Timers { get; }

    /// <summary>
    /// Records an operation with its duration and success status
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="duration">Duration of the operation</param>
    /// <param name="success">Whether the operation was successful</param>
    void RecordOperation(string operationName, TimeSpan duration, bool success);

    /// <summary>
    /// Records a custom metric value
    /// </summary>
    /// <param name="metricName">Name of the metric</param>
    /// <param name="value">Metric value</param>
    void RecordCustomMetric(string metricName, double value);

    /// <summary>
    /// Increments a counter by the specified amount
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <param name="increment">The amount to increment by</param>
    /// <returns>The new counter value</returns>
    long IncrementCounter(string name, long increment = 1);

    /// <summary>
    /// Starts a timer with the specified name
    /// </summary>
    /// <param name="name">Timer name</param>
    void StartTimer(string name);

    /// <summary>
    /// Stops a timer with the specified name
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <returns>The elapsed time</returns>
    TimeSpan StopTimer(string name);

    /// <summary>
    /// Records the time taken by an operation
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <param name="elapsedTime">The elapsed time</param>
    void RecordTime(string name, TimeSpan elapsedTime);

    /// <summary>
    /// Gets the value of a counter
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <returns>The counter value, or 0 if the counter doesn't exist</returns>
    long GetCounter(string name);

    /// <summary>
    /// Gets the elapsed time of a timer
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <returns>The elapsed time, or TimeSpan.Zero if the timer doesn't exist</returns>
    TimeSpan GetTimerElapsed(string name);

    /// <summary>
    /// Creates a snapshot of the current metrics
    /// </summary>
    /// <returns>A dictionary containing all metrics</returns>
    IDictionary<string, object> CreateSnapshot();
}
