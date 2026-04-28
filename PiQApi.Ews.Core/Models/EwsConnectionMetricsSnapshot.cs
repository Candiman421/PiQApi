// PiQApi.Ews.Core/Models/EwsConnectionMetricsSnapshot.cs
using PiQApi.Abstractions.Monitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace PiQApi.Ews.Core.Models
{
    /// <summary>
    /// Snapshot of EWS connection metrics at a specific point in time
    /// Implements IPiQMetricsSnapshot for metrics reporting
    /// </summary>
    public class EwsConnectionMetricsSnapshot : IPiQMetricsSnapshot
    {
        private readonly ReadOnlyDictionary<string, object> _metrics;
        private readonly ReadOnlyDictionary<string, long> _failureReasons;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsConnectionMetricsSnapshot"/> class
        /// </summary>
        /// <param name="connectionsCreated">Number of connections created</param>
        /// <param name="connectionsAcquired">Number of connections acquired</param>
        /// <param name="connectionsReleased">Number of connections released</param>
        /// <param name="connectionFailures">Number of connection failures</param>
        /// <param name="totalConnections">Total connections ever acquired</param>
        /// <param name="activeConnections">Currently active connections</param>
        /// <param name="failedConnections">Failed connections</param>
        /// <param name="lastFailureTime">Last failure timestamp</param>
        /// <param name="validationFailures">Validation failures</param>
        /// <param name="failureReasons">Dictionary of failure reasons and counts</param>
        /// <param name="timestamp">Snapshot timestamp</param>
        /// <param name="successfulOperations">Successful operations count</param>
        /// <param name="failedOperations">Failed operations count</param>
        /// <param name="totalDurationMs">Total duration in milliseconds</param>
        /// <param name="operationCount">Total operation count</param>
        public EwsConnectionMetricsSnapshot(
            long connectionsCreated,
            long connectionsAcquired,
            long connectionsReleased,
            long connectionFailures,
            int totalConnections,
            int activeConnections,
            int failedConnections,
            DateTime lastFailureTime,
            long validationFailures,
            IDictionary<string, long> failureReasons,
            DateTimeOffset timestamp = default,
            long successfulOperations = 0,
            long failedOperations = 0,
            long totalDurationMs = 0,
            long operationCount = 0)
        {
            // Initialize data
            ConnectionsCreated = connectionsCreated;
            ConnectionsAcquired = connectionsAcquired;
            ConnectionsReleased = connectionsReleased;
            ConnectionFailures = connectionFailures;
            TotalConnections = totalConnections;
            ActiveConnections = activeConnections;
            FailedConnections = failedConnections;
            LastFailureTime = lastFailureTime;
            ValidationFailures = validationFailures;

            // Create immutable copy of failure reasons
            _failureReasons = failureReasons != null
                ? new ReadOnlyDictionary<string, long>(new Dictionary<string, long>(failureReasons))
                : new ReadOnlyDictionary<string, long>(new Dictionary<string, long>());

            // Set IPiQMetricsSnapshot properties
            Timestamp = timestamp == default ? DateTimeOffset.UtcNow : timestamp;
            SuccessfulOperations = successfulOperations;
            FailedOperations = failedOperations;
            TotalDurationMs = totalDurationMs;
            OperationCount = operationCount > 0 ? operationCount : (successfulOperations + failedOperations);

            // Build metrics dictionary
            var metrics = new Dictionary<string, object>
            {
                ["ConnectionsCreated"] = ConnectionsCreated,
                ["ConnectionsAcquired"] = ConnectionsAcquired,
                ["ConnectionsReleased"] = ConnectionsReleased,
                ["ConnectionFailures"] = ConnectionFailures,
                ["TotalConnections"] = TotalConnections,
                ["ActiveConnections"] = ActiveConnections,
                ["FailedConnections"] = FailedConnections,
                ["ValidationFailures"] = ValidationFailures,
                ["LastFailureTime"] = LastFailureTime,
                ["SuccessfulOperations"] = SuccessfulOperations,
                ["FailedOperations"] = FailedOperations,
                ["TotalDurationMs"] = TotalDurationMs,
                ["OperationCount"] = OperationCount,
                ["SuccessRate"] = SuccessRate,
                ["AverageOperationDurationMs"] = AverageOperationDurationMs
            };

            // Add failure reasons
            foreach (var reason in _failureReasons)
            {
                metrics[$"FailureReason_{reason.Key}"] = reason.Value;
            }

            _metrics = new ReadOnlyDictionary<string, object>(metrics);
        }

        // Constructor for creating from another snapshot with additional metrics
        private EwsConnectionMetricsSnapshot(
            EwsConnectionMetricsSnapshot original,
            IReadOnlyDictionary<string, object> additionalMetrics)
        {
            // Copy properties from original
            ConnectionsCreated = original.ConnectionsCreated;
            ConnectionsAcquired = original.ConnectionsAcquired;
            ConnectionsReleased = original.ConnectionsReleased;
            ConnectionFailures = original.ConnectionFailures;
            TotalConnections = original.TotalConnections;
            ActiveConnections = original.ActiveConnections;
            FailedConnections = original.FailedConnections;
            LastFailureTime = original.LastFailureTime;
            ValidationFailures = original.ValidationFailures;
            _failureReasons = original._failureReasons;

            // Copy IPiQMetricsSnapshot properties
            Timestamp = original.Timestamp;
            SuccessfulOperations = original.SuccessfulOperations;
            FailedOperations = original.FailedOperations;
            TotalDurationMs = original.TotalDurationMs;
            OperationCount = original.OperationCount;

            // Merge metrics
            var metrics = new Dictionary<string, object>(original._metrics);

            if (additionalMetrics != null)
            {
                foreach (var metric in additionalMetrics)
                {
                    metrics[metric.Key] = metric.Value;
                }
            }

            _metrics = new ReadOnlyDictionary<string, object>(metrics);
        }

        /// <summary>
        /// Gets the number of connections created
        /// </summary>
        public long ConnectionsCreated { get; }

        /// <summary>
        /// Gets the number of connections acquired
        /// </summary>
        public long ConnectionsAcquired { get; }

        /// <summary>
        /// Gets the number of connections released
        /// </summary>
        public long ConnectionsReleased { get; }

        /// <summary>
        /// Gets the number of connection failures
        /// </summary>
        public long ConnectionFailures { get; }

        /// <summary>
        /// Gets the total number of connections ever acquired
        /// </summary>
        public int TotalConnections { get; }

        /// <summary>
        /// Gets the number of currently active connections
        /// </summary>
        public int ActiveConnections { get; }

        /// <summary>
        /// Gets the number of failed connections
        /// </summary>
        public int FailedConnections { get; }

        /// <summary>
        /// Gets the timestamp of the last failure
        /// </summary>
        public DateTime LastFailureTime { get; }

        /// <summary>
        /// Gets the number of validation failures
        /// </summary>
        public long ValidationFailures { get; }

        /// <summary>
        /// Gets the dictionary of failure reasons and their counts
        /// </summary>
        public IReadOnlyDictionary<string, long> FailureReasons => _failureReasons;

        #region IPiQMetricsSnapshot Implementation

        /// <summary>
        /// Gets the timestamp when the snapshot was created
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the metrics dictionary containing all tracked metrics
        /// </summary>
        public IReadOnlyDictionary<string, object> Metrics => _metrics;

        /// <summary>
        /// Gets the number of successful operations
        /// </summary>
        public long SuccessfulOperations { get; }

        /// <summary>
        /// Gets the number of failed operations
        /// </summary>
        public long FailedOperations { get; }

        /// <summary>
        /// Gets the total duration of all operations in milliseconds
        /// </summary>
        public long TotalDurationMs { get; }

        /// <summary>
        /// Gets the total number of operations
        /// </summary>
        public long OperationCount { get; }

        /// <summary>
        /// Gets the success rate as a percentage
        /// </summary>
        public double SuccessRate => OperationCount > 0
            ? (double)SuccessfulOperations / OperationCount * 100.0
            : 0.0;

        /// <summary>
        /// Gets the average operation duration in milliseconds
        /// </summary>
        public long AverageOperationDurationMs => OperationCount > 0
            ? TotalDurationMs / OperationCount
            : 0;

        /// <summary>
        /// Creates a new snapshot with an additional metric
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="value">Metric value</param>
        /// <returns>A new snapshot with the additional metric</returns>
        public IPiQMetricsSnapshot WithMetric(string name, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(value);

            var additionalMetrics = new Dictionary<string, object> { [name] = value };
            return new EwsConnectionMetricsSnapshot(this, additionalMetrics);
        }

        /// <summary>
        /// Creates a new snapshot with additional metrics
        /// </summary>
        /// <param name="metrics">Metrics to add</param>
        /// <returns>A new snapshot with the additional metrics</returns>
        public IPiQMetricsSnapshot WithMetrics(IDictionary<string, object> metrics)
        {
            ArgumentNullException.ThrowIfNull(metrics);

            return new EwsConnectionMetricsSnapshot(this,
                new ReadOnlyDictionary<string, object>(
                    metrics.Where(kvp => !string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
        }

        /// <summary>
        /// Retrieves a metric value by name
        /// </summary>
        /// <typeparam name="T">Type of the metric value</typeparam>
        /// <param name="name">Metric name</param>
        /// <returns>The metric value</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the metric is not found</exception>
        /// <exception cref="InvalidCastException">Thrown when the metric cannot be converted to the specified type</exception>
        public T GetMetric<T>(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            if (!_metrics.TryGetValue(name, out var value))
            {
                throw new KeyNotFoundException($"Metric '{name}' not found in snapshot");
            }

            if (value is T typedValue)
            {
                return typedValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
            {
                throw new InvalidCastException(
                    $"Cannot convert metric '{name}' from type {value?.GetType().Name ?? "null"} to {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Attempts to retrieve a metric value by name
        /// </summary>
        /// <typeparam name="T">Type of the metric value</typeparam>
        /// <param name="name">Metric name</param>
        /// <param name="value">Output parameter for the metric value</param>
        /// <returns>True if the metric exists and can be cast to the specified type, otherwise false</returns>
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
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the snapshot contains a metric with the specified name
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <returns>True if the metric exists, otherwise false</returns>
        public bool HasMetric(string name)
        {
            return !string.IsNullOrEmpty(name) && _metrics.ContainsKey(name);
        }

        #endregion
    }
}