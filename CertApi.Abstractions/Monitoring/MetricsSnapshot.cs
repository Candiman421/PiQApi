// CertApi.Abstractions/Monitoring/MetricsSnapshot.cs
namespace CertApi.Abstractions.Monitoring
{
    /// <summary>
    /// Immutable snapshot of metrics at a point in time
    /// </summary>
    public sealed class MetricsSnapshot
    {
        /// <summary>
        /// Gets the timestamp when this snapshot was created
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the total number of connections ever acquired
        /// </summary>
        public int TotalConnections { get; }

        /// <summary>
        /// Gets the number of currently active connections
        /// </summary>
        public int ActiveConnections { get; }

        /// <summary>
        /// Gets the number of connections that have failed
        /// </summary>
        public int FailedConnections { get; }

        /// <summary>
        /// Gets the total number of operations (successful + failed)
        /// </summary>
        public int OperationCount { get; }

        /// <summary>
        /// Gets the average duration of operations in milliseconds
        /// </summary>
        public double AverageOperationDurationMs { get; }

        /// <summary>
        /// Gets the percentage of successful operations
        /// </summary>
        public double SuccessRate { get; }

        /// <summary>
        /// Gets the average latency for network operations
        /// </summary>
        public TimeSpan AverageLatency { get; }

        /// <summary>
        /// Gets the percentage of cache hits vs total cache operations
        /// </summary>
        public double CacheHitRate { get; }

        /// <summary>
        /// Gets custom metrics collected by the application
        /// </summary>
        public IReadOnlyDictionary<string, double> CustomMetrics { get; }

        /// <summary>
        /// Initializes a new instance of the MetricsSnapshot class
        /// </summary>
        public MetricsSnapshot(
            int totalConnections = 0,
            int activeConnections = 0,
            int failedConnections = 0,
            int operationCount = 0,
            double averageOperationDurationMs = 0,
            double successRate = 0,
            TimeSpan? averageLatency = null,
            double cacheHitRate = 0,
            IReadOnlyDictionary<string, double>? customMetrics = null)
        {
            Timestamp = DateTimeOffset.UtcNow;
            TotalConnections = Math.Max(0, totalConnections);
            ActiveConnections = Math.Max(0, activeConnections);
            FailedConnections = Math.Max(0, failedConnections);
            OperationCount = Math.Max(0, operationCount);
            AverageOperationDurationMs = Math.Max(0, averageOperationDurationMs);
            SuccessRate = Math.Clamp(successRate, 0, 100);
            AverageLatency = averageLatency ?? TimeSpan.Zero;
            CacheHitRate = Math.Clamp(cacheHitRate, 0, 100);
            
            // Create a defensive copy of custom metrics to prevent external modification
            CustomMetrics = customMetrics != null 
                ? new Dictionary<string, double>(customMetrics) 
                : new Dictionary<string, double>();
        }
    }
}