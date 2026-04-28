// CertApi.Abstractions/Monitoring/IConnectionMetrics.cs
namespace CertApi.Abstractions.Monitoring
{
    /// <summary>
    /// Provides metrics tracking for connection management
    /// </summary>
    public interface IConnectionMetrics : IMetricsProvider
    {
        /// <summary>
        /// Gets the total number of connections ever acquired
        /// </summary>
        int TotalConnections { get; }

        /// <summary>
        /// Gets the number of currently active connections
        /// </summary>
        int ActiveConnections { get; }

        /// <summary>
        /// Gets the number of connections that have failed
        /// </summary>
        int FailedConnections { get; }

        /// <summary>
        /// Gets the timestamp of the last connection failure
        /// </summary>
        DateTime LastFailureTime { get; }

        /// <summary>
        /// Records a new connection acquisition
        /// </summary>
        void RecordConnectionAcquired(TimeSpan acquisitionTime);

        /// <summary>
        /// Records a connection being released
        /// </summary>
        void RecordConnectionReleased(TimeSpan lifetime);

        /// <summary>
        /// Records a connection failure
        /// </summary>
        void RecordConnectionFailure(string errorCode);
    }
}