// PiQApi.Core/Monitoring/NoOpOperationMetrics.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Monitoring;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// No-operation implementation of IPiQOperationMetrics for use when metrics are disabled
/// </summary>
public sealed class NoOpOperationMetrics : IPiQOperationMetrics
{
    private static readonly NoOpOperationMetrics _instance = new NoOpOperationMetrics();
    private static readonly Dictionary<string, object> _emptySnapshot = new Dictionary<string, object>();
    private static readonly Dictionary<string, long> _emptyCounters = new Dictionary<string, long>();
    private static readonly Dictionary<string, TimeSpan> _emptyTimers = new Dictionary<string, TimeSpan>();
    private static readonly Dictionary<string, double> _emptyCustomMetrics = new Dictionary<string, double>();
    private static readonly PiQMetricsSnapshot _emptyMetricsSnapshot = new PiQMetricsSnapshot();

    /// <summary>
    /// Gets the singleton instance of NoOpOperationMetrics
    /// </summary>
    public static NoOpOperationMetrics Instance => _instance;

    /// <summary>
    /// Private constructor to enforce singleton pattern
    /// </summary>
    private NoOpOperationMetrics() { }

    /// <inheritdoc/>
    public long SuccessfulOperations => 0;

    /// <inheritdoc/>
    public long FailedOperations => 0;

    /// <inheritdoc/>
    public long OperationCount => 0;

    /// <inheritdoc/>
    public double SuccessRate => 0;

    /// <inheritdoc/>
    public long AverageOperationDurationMs => 0;

    /// <inheritdoc/>
    public TimeSpan Duration => TimeSpan.Zero;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, double> CustomMetrics => _emptyCustomMetrics;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, long> Counters => _emptyCounters;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, TimeSpan> Timers => _emptyTimers;

    /// <inheritdoc/>
    public void RecordOperation(string operationName, TimeSpan duration, bool success) { }

    /// <inheritdoc/>
    public void RecordCustomMetric(string metricName, double value) { }

    /// <inheritdoc/>
    public long IncrementCounter(string name, long increment = 1) => 0;

    /// <inheritdoc/>
    public void StartTimer(string name) { }

    /// <inheritdoc/>
    public TimeSpan StopTimer(string name) => TimeSpan.Zero;

    /// <inheritdoc/>
    public void RecordTime(string name, TimeSpan elapsedTime) { }

    /// <inheritdoc/>
    public void ResetCounters() { }

    /// <inheritdoc/>
    public long GetCounter(string name) => 0;

    /// <inheritdoc/>
    public TimeSpan GetTimerElapsed(string name) => TimeSpan.Zero;

    /// <inheritdoc/>
    public IDictionary<string, object> CreateSnapshot() => _emptySnapshot;

    /// <inheritdoc/>
    public Task<IPiQMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IPiQMetricsSnapshot>(_emptyMetricsSnapshot);
}
