// CertApi.Abstractions/Monitoring/ICompositeMetricsTracker.cs
namespace CertApi.Abstractions.Monitoring
{
    /// <summary>
    /// Combined interface for comprehensive metrics tracking
    /// </summary>
    public interface ICompositeMetricsTracker : IConnectionMetrics, IOperationMetrics, ICacheMetrics
    {
        /// <summary>
        /// Records a custom metric
        /// </summary>
        void RecordCustomMetric(string name, double value);
    }
}