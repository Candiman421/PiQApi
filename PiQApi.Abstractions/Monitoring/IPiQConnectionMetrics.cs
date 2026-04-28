// PiQApi.Abstractions/Monitoring/IPiQConnectionMetrics.cs
namespace PiQApi.Abstractions.Monitoring;

/// <summary>
/// Provides metrics tracking for connection management
/// </summary>
public interface IPiQConnectionMetrics : IPiQMetricsProvider
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
    /// <param name="acquisitionTime">Time taken to acquire the connection</param>
    void RecordConnectionAcquired(TimeSpan acquisitionTime);

    /// <summary>
    /// Records a connection being released
    /// </summary>
    /// <param name="lifetime">Total lifetime of the connection</param>
    void RecordConnectionReleased(TimeSpan lifetime);

    /// <summary>
    /// Records a connection failure
    /// </summary>
    /// <param name="errorCode">Unique error code for the connection failure</param>
    void RecordConnectionFailure(string errorCode);
}