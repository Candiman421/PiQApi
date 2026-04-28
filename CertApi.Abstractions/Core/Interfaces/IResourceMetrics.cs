// CertApi.Abstractions/Core/Interfaces/IResourceMetrics.cs
using CertApi.Abstractions.Monitoring;

namespace CertApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Tracks and provides metrics for system resources
    /// </summary>
    public interface IResourceMetrics
    {
        /// <summary>
        /// Gets the current metrics snapshot
        /// </summary>
        /// <returns>Metrics snapshot</returns>
        Task<MetricsSnapshot> GetMetricsSnapshotAsync();

        /// <summary>
        /// Records resource usage for an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="success">Whether the operation was successful</param>
        void RecordOperation(string operationName, TimeSpan duration, bool success);
    }
}