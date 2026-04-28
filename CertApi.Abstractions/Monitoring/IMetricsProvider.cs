// CertApi.Abstractions/Monitoring/IMetricsProvider.cs
namespace CertApi.Abstractions.Monitoring
{
    /// <summary>
    /// Base interface for all metrics providers
    /// </summary>
    public interface IMetricsProvider
    {
        /// <summary>
        /// Gets a snapshot of the current metrics
        /// </summary>
        Task<MetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets volatile counters while preserving persistent state
        /// </summary>
        void ResetCounters();
    }
}