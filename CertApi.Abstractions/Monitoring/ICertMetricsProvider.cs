// CertApi.Abstractions/Monitoring/ICertMetricsProvider.cs
namespace CertApi.Abstractions.Monitoring;

/// <summary>
/// Provides access to metrics snapshots with comprehensive tracking capabilities
/// </summary>
public interface ICertMetricsProvider
{
    /// <summary>
    /// Gets a snapshot of the current metrics asynchronously
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task containing the metrics snapshot</returns>
    Task<ICertMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets all tracked metrics to their initial state
    /// </summary>
    void ResetCounters();
}