// PiQApi.Core/Monitoring/CertOperationMetricsFactory.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Monitoring;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Utilities.RandomProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// Factory for creating operation metrics trackers
/// </summary>
public class CertOperationMetricsFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ICertTimeProvider _timeProvider;
    private readonly CertOperationMetricsOptions _options;

    /// <summary>
    /// Creates a new instance of the CertOperationMetricsFactory
    /// </summary>
    /// <param name="loggerFactory">Logger factory for creating loggers</param>
    /// <param name="timeProvider">Time provider for metrics</param>
    /// <param name="options">Metrics configuration options</param>
    public CertOperationMetricsFactory(
        ILoggerFactory loggerFactory,
        ICertTimeProvider timeProvider,
        IOptions<CertOperationMetricsOptions> options)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Validate options
        _options.Validate();
    }

    /// <summary>
    /// Creates a new operation metrics tracker
    /// </summary>
    /// <param name="correlationContext">Optional correlation context</param>
    /// <returns>A new operation metrics tracker</returns>
    public ICertOperationMetrics CreateTracker(ICertCorrelationContext? correlationContext = null)
    {
        // If metrics are disabled, return a no-op implementation
        if (!_options.IsEnabled)
        {
            return NoOpOperationMetrics.Instance;
        }

        // Apply sampling - use secure random for predictability avoidance
        // Use the fast provider since this isn't security sensitive
        if (_options.SamplingRate < 1.0 &&
            CertRandomProviderFactory.FastProvider.NextDouble() > _options.SamplingRate)
        {
            return NoOpOperationMetrics.Instance;
        }

        // Create standard metrics tracker with appropriate logger
        var logger = _loggerFactory.CreateLogger<CertOperationMetricsTracker>();
        var tracker = new CertOperationMetricsTracker(logger, _timeProvider);

        // If correlation context is provided, add correlation information
        if (correlationContext != null)
        {
            string correlationId = correlationContext.CorrelationId;
            if (!string.IsNullOrEmpty(correlationId))
            {
                // Calculate a hash code that uses an explicit string comparison
                tracker.RecordCustomMetric("CorrelationId", ComputeStableStringHash(correlationId));

                // Add any parent correlation ID if available
                var parentIdEntry = correlationContext.Properties
                    .FirstOrDefault(p => string.Equals(p.Key, "ParentCorrelationId", StringComparison.Ordinal));

                if (parentIdEntry.Key != null && parentIdEntry.Value is string parentIdString)
                {
                    // Calculate a hash code that uses an explicit string comparison
                    tracker.RecordCustomMetric("ParentCorrelationId", ComputeStableStringHash(parentIdString));
                }
            }
        }

        return tracker;
    }

    /// <summary>
    /// Computes a stable hash code for a string using explicit ordinal comparison
    /// </summary>
    /// <param name="s">String to hash</param>
    /// <returns>A stable hash code</returns>
    private static int ComputeStableStringHash(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return 0;
        }

        // Use an explicit hashing algorithm that's stable across environments
        // This is a simple FNV-1a hash algorithm
        const int fnvPrime = 16777619;
        const int fnvOffsetBasis = unchecked((int)2166136261);

        int hash = fnvOffsetBasis;

        for (int i = 0; i < s.Length; i++)
        {
            // Use ordinal comparison by using the raw character value
            hash = unchecked(hash ^ s[i]);
            hash = unchecked(hash * fnvPrime);
        }

        return hash;
    }

    /// <summary>
    /// Creates a new cache metrics tracker
    /// </summary>
    /// <returns>A new cache metrics tracker</returns>
    public ICertCacheMetrics CreateCacheTracker()
    {
        // If metrics are disabled, return a no-op implementation that implements ICertCacheMetrics
        if (!_options.IsEnabled)
        {
            return new CertCompositeMetricsTracker(
                _loggerFactory.CreateLogger<CertCompositeMetricsTracker>(),
                _timeProvider);
        }

        // Create standard cache metrics tracker
        var logger = _loggerFactory.CreateLogger<CertCacheMetricsTracker>();
        return new CertCacheMetricsTracker(logger, _timeProvider);
    }

    /// <summary>
    /// Creates a new connection metrics tracker
    /// </summary>
    /// <returns>A new connection metrics tracker</returns>
    public ICertConnectionMetrics CreateConnectionTracker()
    {
        // If metrics are disabled, return a no-op implementation that implements ICertConnectionMetrics
        if (!_options.IsEnabled)
        {
            return new CertCompositeMetricsTracker(
                _loggerFactory.CreateLogger<CertCompositeMetricsTracker>(),
                _timeProvider);
        }

        // Create standard connection metrics tracker
        var logger = _loggerFactory.CreateLogger<CertConnectionMetricsTracker>();
        return new CertConnectionMetricsTracker(logger, _timeProvider);
    }

    /// <summary>
    /// Creates a new composite metrics tracker
    /// </summary>
    /// <returns>A new composite metrics tracker</returns>
    public ICertCompositeMetricsTracker CreateCompositeTracker()
    {
        // Create composite metrics tracker (will be no-op internally if metrics are disabled)
        var logger = _loggerFactory.CreateLogger<CertCompositeMetricsTracker>();
        return new CertCompositeMetricsTracker(logger, _timeProvider);
    }
}
