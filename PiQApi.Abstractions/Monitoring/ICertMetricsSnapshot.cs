// PiQApi.Abstractions/Monitoring/ICertMetricsSnapshot.cs
namespace PiQApi.Abstractions.Monitoring;

/// <summary>
/// Represents an immutable snapshot of metrics at a specific point in time
/// </summary>
public interface ICertMetricsSnapshot
{
    /// <summary>
    /// Gets the timestamp when the snapshot was created
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the metrics dictionary containing all tracked metrics
    /// </summary>
    IReadOnlyDictionary<string, object> Metrics { get; }

    /// <summary>
    /// Gets the number of successful operations
    /// </summary>
    long SuccessfulOperations { get; }

    /// <summary>
    /// Gets the number of failed operations
    /// </summary>
    long FailedOperations { get; }

    /// <summary>
    /// Gets the total duration of all operations in milliseconds
    /// </summary>
    long TotalDurationMs { get; }

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
    /// Creates a new snapshot with an additional metric
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <returns>A new snapshot with the additional metric</returns>
    ICertMetricsSnapshot WithMetric(string name, object value);

    /// <summary>
    /// Creates a new snapshot with additional metrics
    /// </summary>
    /// <param name="metrics">Metrics to add</param>
    /// <returns>A new snapshot with the additional metrics</returns>
    ICertMetricsSnapshot WithMetrics(IDictionary<string, object> metrics);

    /// <summary>
    /// Retrieves a metric value by name
    /// </summary>
    /// <typeparam name="T">Type of the metric value</typeparam>
    /// <param name="name">Metric name</param>
    /// <returns>The metric value</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the metric is not found</exception>
    /// <exception cref="InvalidCastException">Thrown when the metric cannot be converted to the specified type</exception>
    T GetMetric<T>(string name);

    /// <summary>
    /// Attempts to retrieve a metric value by name
    /// </summary>
    /// <typeparam name="T">Type of the metric value</typeparam>
    /// <param name="name">Metric name</param>
    /// <param name="value">Output parameter for the metric value</param>
    /// <returns>True if the metric exists and can be cast to the specified type, otherwise false</returns>
    bool TryGetMetric<T>(string name, out T? value);

    /// <summary>
    /// Checks if the snapshot contains a metric with the specified name
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <returns>True if the metric exists, otherwise false</returns>
    bool HasMetric(string name);
}