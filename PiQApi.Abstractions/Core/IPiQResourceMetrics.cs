// PiQApi.Abstractions/Core/IPiQResourceMetrics.cs
using PiQApi.Abstractions.Monitoring;

namespace PiQApi.Abstractions.Core;

/// <summary>
/// Tracks and provides metrics for system resources
/// </summary>
public interface IPiQResourceMetrics
{
    /// <summary>
    /// Gets the total number of resources currently in use
    /// </summary>
    int TotalResourcesInUse { get; }

    /// <summary>
    /// Gets the current metrics snapshot
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Metrics snapshot</returns>
    Task<IPiQMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records resource usage for an operation
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="duration">Duration of the operation</param>
    /// <param name="success">Whether the operation was successful</param>
    void RecordOperation(string operationName, TimeSpan duration, bool success);

    /// <summary>
    /// Records a resource acquisition
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="resourceId">Resource identifier</param>
    void RecordAcquisition(string resourceType, string resourceId);

    /// <summary>
    /// Records a resource release
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="resourceId">Resource identifier</param>
    void RecordRelease(string resourceType, string resourceId);

    /// <summary>
    /// Records a resource operation
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="operationType">Type of operation</param>
    /// <param name="duration">Duration of the operation</param>
    void RecordOperation(string resourceType, string operationType, TimeSpan duration);

    /// <summary>
    /// Records a resource error
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="errorType">Type of error</param>
    void RecordError(string resourceType, string errorType);

    /// <summary>
    /// Increments a counter metric
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <param name="increment">Increment amount</param>
    void IncrementCounter(string name, long increment = 1);

    /// <summary>
    /// Records a resource acquisition and returns the new count
    /// </summary>
    /// <param name="resourceType">The type of resource acquired</param>
    /// <returns>The new count of resources in use</returns>
    int RecordResourceAcquired(string resourceType);

    /// <summary>
    /// Records a resource release and returns the new count
    /// </summary>
    /// <param name="resourceType">The type of resource released</param>
    /// <returns>The new count of resources in use</returns>
    int RecordResourceReleased(string resourceType);
}
