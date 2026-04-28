// PiQApi.Abstractions/Monitoring/IPiQCompositeMetricsTracker.cs
using PiQApi.Abstractions.Context;

namespace PiQApi.Abstractions.Monitoring;

/// <summary>
/// Combined interface for comprehensive metrics tracking
/// </summary>
public interface IPiQCompositeMetricsTracker :
    IPiQConnectionMetrics,
    IPiQCacheMetrics,
    IPiQMetricsProvider
{
    /// <summary>
    /// Records a custom metric value
    /// </summary>
    /// <param name="name">Name of the metric</param>
    /// <param name="value">Value of the metric</param>
    void RecordCustomMetric(string name, double value);

    /// <summary>
    /// Adds a metrics tracker to the composite
    /// </summary>
    /// <param name="tracker">Metrics tracker to add</param>
    void AddTracker(IPiQOperationMetrics tracker);
}