// PiQApi.Core/Monitoring/CertMetricsSnapshot.cs
using System.Collections.Immutable;
using System.Globalization;
using PiQApi.Abstractions.Monitoring;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// Represents an immutable snapshot of metrics at a specific point in time
/// </summary>
public sealed class CertMetricsSnapshot : ICertMetricsSnapshot, IEquatable<CertMetricsSnapshot>
{
    private readonly ImmutableDictionary<string, object> _metrics;

    /// <summary>
    /// Gets the timestamp when the snapshot was created
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the metrics dictionary
    /// </summary>
    public IReadOnlyDictionary<string, object> Metrics => _metrics;

    /// <summary>
    /// Gets the number of successful operations
    /// </summary>
    public long SuccessfulOperations { get; set; }

    /// <summary>
    /// Gets the number of failed operations
    /// </summary>
    public long FailedOperations { get; set; }

    /// <summary>
    /// Gets the total duration of all operations in milliseconds
    /// </summary>
    public long TotalDurationMs { get; set; }

    /// <summary>
    /// Gets the total number of operations
    /// </summary>
    public long OperationCount { get; set; }

    /// <summary>
    /// Gets the success rate as a percentage
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Gets the average operation duration in milliseconds
    /// </summary>
    public long AverageOperationDurationMs { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertMetricsSnapshot"/> class
    /// </summary>
    public CertMetricsSnapshot()
        : this(ImmutableDictionary<string, object>.Empty, DateTimeOffset.UtcNow)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertMetricsSnapshot"/> class with metrics
    /// </summary>
    /// <param name="metrics">Initial metrics dictionary</param>
    public CertMetricsSnapshot(IDictionary<string, object> metrics)
        : this(metrics?.ToImmutableDictionary() ?? ImmutableDictionary<string, object>.Empty, DateTimeOffset.UtcNow)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertMetricsSnapshot"/> class with the specified timestamp
    /// </summary>
    /// <param name="metrics">Metrics dictionary</param>
    /// <param name="timestamp">Timestamp</param>
    public CertMetricsSnapshot(ImmutableDictionary<string, object> metrics, DateTimeOffset timestamp)
    {
        _metrics = metrics ?? ImmutableDictionary<string, object>.Empty;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertMetricsSnapshot"/> class with common metrics
    /// </summary>
    /// <param name="totalConnections">Total connections count</param>
    /// <param name="activeConnections">Active connections count</param>
    /// <param name="failedConnections">Failed connections count</param>
    /// <param name="cacheHitRate">Cache hit rate percentage</param>
    /// <param name="operationCount">Total operations count</param>
    /// <param name="averageOperationDurationMs">Average operation duration in milliseconds</param>
    /// <param name="successRate">Success rate percentage</param>
    public CertMetricsSnapshot(
        int totalConnections = 0,
        int activeConnections = 0,
        int failedConnections = 0,
        double cacheHitRate = 0.0,
        int operationCount = 0,
        double averageOperationDurationMs = 0.0,
        double successRate = 0.0)
        : this(ImmutableDictionary.CreateRange(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["TotalConnections"] = totalConnections,
            ["ActiveConnections"] = activeConnections,
            ["FailedConnections"] = failedConnections,
            ["CacheHitRate"] = cacheHitRate,
            ["OperationCount"] = operationCount,
            ["AverageOperationDurationMs"] = averageOperationDurationMs,
            ["SuccessRate"] = successRate
        }), DateTimeOffset.UtcNow)
    {
        OperationCount = operationCount;
        AverageOperationDurationMs = (long)averageOperationDurationMs;
        SuccessRate = successRate;
    }

    /// <summary>
    /// Creates a new snapshot with an additional metric
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <returns>New snapshot with the additional metric</returns>
    public ICertMetricsSnapshot WithMetric(string name, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(value);

        return new CertMetricsSnapshot(_metrics.SetItem(name, value), Timestamp)
        {
            SuccessfulOperations = SuccessfulOperations,
            FailedOperations = FailedOperations,
            TotalDurationMs = TotalDurationMs,
            OperationCount = OperationCount,
            SuccessRate = SuccessRate,
            AverageOperationDurationMs = AverageOperationDurationMs
        };
    }

    /// <summary>
    /// Creates a new snapshot with additional metrics
    /// </summary>
    /// <param name="metrics">Metrics to add</param>
    /// <returns>New snapshot with the additional metrics</returns>
    public ICertMetricsSnapshot WithMetrics(IDictionary<string, object> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var builder = _metrics.ToBuilder();
        foreach (var kvp in metrics)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value is not null)
            {
                builder[kvp.Key] = kvp.Value;
            }
        }

        return new CertMetricsSnapshot(builder.ToImmutable(), Timestamp)
        {
            SuccessfulOperations = SuccessfulOperations,
            FailedOperations = FailedOperations,
            TotalDurationMs = TotalDurationMs,
            OperationCount = OperationCount,
            SuccessRate = SuccessRate,
            AverageOperationDurationMs = AverageOperationDurationMs
        };
    }

    /// <summary>
    /// Gets a metric value by name
    /// </summary>
    /// <typeparam name="T">Type of the metric value</typeparam>
    /// <param name="name">Metric name</param>
    /// <returns>Metric value</returns>
    public T GetMetric<T>(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (_metrics.TryGetValue(name, out var value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
            {
                throw new InvalidCastException($"Cannot convert metric '{name}' to type {typeof(T).Name}", ex);
            }
        }

        throw new KeyNotFoundException($"Metric '{name}' not found");
    }

    /// <summary>
    /// Tries to get a metric value by name
    /// </summary>
    /// <typeparam name="T">Type of the metric value</typeparam>
    /// <param name="name">Metric name</param>
    /// <param name="value">Output metric value</param>
    /// <returns>True if the metric exists and can be cast to the specified type</returns>
    public bool TryGetMetric<T>(string name, out T? value)
    {
        value = default;

        if (string.IsNullOrEmpty(name) || !_metrics.TryGetValue(name, out var metricValue))
        {
            return false;
        }

        if (metricValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        try
        {
            value = (T)Convert.ChangeType(metricValue, typeof(T), CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the snapshot contains a metric with the specified name
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <returns>True if the metric exists</returns>
    public bool HasMetric(string name)
    {
        return !string.IsNullOrEmpty(name) && _metrics.ContainsKey(name);
    }

    /// <summary>
    /// Determines if this snapshot is equal to another
    /// </summary>
    public bool Equals(CertMetricsSnapshot? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        if (Timestamp != other.Timestamp || _metrics.Count != other._metrics.Count)
            return false;

        return _metrics.Keys.All(key =>
            other._metrics.TryGetValue(key, out var otherValue) &&
            Equals(_metrics[key], otherValue));
    }

    /// <summary>
    /// Determines if this snapshot is equal to another object
    /// </summary>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is CertMetricsSnapshot other && Equals(other);
    }

    /// <summary>
    /// Gets a hash code for the snapshot
    /// </summary>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Timestamp);

        foreach (var kvp in _metrics.OrderBy(k => k.Key))
        {
            hashCode.Add(kvp.Key);
            if (kvp.Value != null)
            {
                hashCode.Add(kvp.Value);
            }
        }

        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(CertMetricsSnapshot? left, CertMetricsSnapshot? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(CertMetricsSnapshot? left, CertMetricsSnapshot? right)
    {
        return !Equals(left, right);
    }
}