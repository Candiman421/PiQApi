// CertApi.Core/Core/CertResourceMetrics.cs
using System.Collections.Concurrent;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Monitoring;
using CertApi.Core.Monitoring;

namespace CertApi.Core.Core;

/// <summary>
/// Thread-safe implementation of ICertResourceMetrics
/// </summary>
public class CertResourceMetrics : ICertResourceMetrics
{
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, TimeSpan> _timers = new();
    private readonly ConcurrentDictionary<string, (long OperationCount, long SuccessCount, TimeSpan TotalDuration)> _operations = new();
    private readonly ConcurrentDictionary<string, int> _resourceCounts = new();
    private int _totalResourcesInUse;

    /// <inheritdoc/>
    public int TotalResourcesInUse => _totalResourcesInUse;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, long> Counters => _counters;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, TimeSpan> Timers => _timers;

    /// <inheritdoc/>
    public void IncrementCounter(string name, long increment = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        _counters.AddOrUpdate(name, increment, (_, existing) => existing + increment);
    }

    /// <inheritdoc/>
    public int RecordResourceAcquired(string resourceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType, nameof(resourceType));

        // Increment the resource type counter
        _resourceCounts.AddOrUpdate(resourceType, 1, (_, count) => count + 1);

        // Increment and return the total resources counter using interlocked operations
        return Interlocked.Increment(ref _totalResourcesInUse);
    }

    /// <inheritdoc/>
    public int RecordResourceReleased(string resourceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType, nameof(resourceType));

        // Decrement the resource type counter, ensuring it never goes below zero
        _resourceCounts.AddOrUpdate(resourceType, 0, (_, count) => Math.Max(0, count - 1));

        // Decrement and return the total resources counter, ensuring it never goes below zero
        int newCount = Interlocked.Decrement(ref _totalResourcesInUse);
        if (newCount < 0)
        {
            Interlocked.Exchange(ref _totalResourcesInUse, 0);
            return 0;
        }

        return newCount;
    }

    /// <inheritdoc/>
    public void RecordTime(string name, TimeSpan elapsed)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        _timers.AddOrUpdate(name, elapsed, (_, existing) => existing + elapsed);
    }

    /// <inheritdoc/>
    public void RecordOperation(string operationName, TimeSpan duration, bool success)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        _operations.AddOrUpdate(operationName,
            // Add new entry
            (1, success ? 1 : 0, duration),
            // Update existing entry
            (_, existing) => (
                existing.OperationCount + 1,
                existing.SuccessCount + (success ? 1 : 0),
                existing.TotalDuration + duration
            )
        );

        // Backward compatibility counters
        IncrementCounter($"Operation_{operationName}_Count");
        IncrementCounter(success
            ? $"Operation_{operationName}_Success"
            : $"Operation_{operationName}_Failure");

        RecordTime($"Operation_{operationName}_Duration", duration);
    }

    /// <inheritdoc/>
    public void RecordAcquisition(string resourceType, string resourceId)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType, nameof(resourceType));
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));

        IncrementCounter($"Resource_{resourceType}_Acquisition");
        IncrementCounter($"Resource_{resourceType}_{resourceId}_Acquisition");
    }

    /// <inheritdoc/>
    public void RecordRelease(string resourceType, string resourceId)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType, nameof(resourceType));
        ArgumentException.ThrowIfNullOrEmpty(resourceId, nameof(resourceId));

        IncrementCounter($"Resource_{resourceType}_Release");
        IncrementCounter($"Resource_{resourceType}_{resourceId}_Release");
    }

    /// <inheritdoc/>
    public void RecordOperation(string resourceType, string operationType, TimeSpan duration)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType, nameof(resourceType));
        ArgumentException.ThrowIfNullOrEmpty(operationType, nameof(operationType));

        string key = $"Resource_{resourceType}_{operationType}";
        IncrementCounter($"{key}_Count");
        RecordTime($"{key}_Duration", duration);
    }

    /// <inheritdoc/>
    public void RecordError(string resourceType, string errorType)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType, nameof(resourceType));
        ArgumentException.ThrowIfNullOrEmpty(errorType, nameof(errorType));

        IncrementCounter($"Resource_{resourceType}_Error");
        IncrementCounter($"Resource_{resourceType}_Error_{errorType}");
    }

    /// <inheritdoc/>
    public void ResetCounters()
    {
        _counters.Clear();
        _timers.Clear();
        _operations.Clear();
        _resourceCounts.Clear();
        Interlocked.Exchange(ref _totalResourcesInUse, 0);
    }

    /// <inheritdoc/>
    public async Task<ICertMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // Check for cancellation
        cancellationToken.ThrowIfCancellationRequested();

        var snapshot = new CertMetricsSnapshot(CreateSnapshot());
        return await Task.FromResult<ICertMetricsSnapshot>(snapshot).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public IDictionary<string, object> CreateSnapshot()
    {
        var snapshot = new Dictionary<string, object>();

        // Add total resources in use
        snapshot["TotalResourcesInUse"] = TotalResourcesInUse;

        // Add resource counts by type
        foreach (var resourceCount in _resourceCounts)
        {
            snapshot[$"ResourceCount_{resourceCount.Key}"] = resourceCount.Value;
        }

        // Add counters
        foreach (var counter in _counters)
        {
            snapshot[$"Counter_{counter.Key}"] = counter.Value;
        }

        // Add timers
        foreach (var timer in _timers)
        {
            snapshot[$"Timer_{timer.Key}"] = timer.Value.TotalMilliseconds;
        }

        // Add operation statistics
        foreach (var operation in _operations)
        {
            var (OperationCount, SuccessCount, TotalDuration) = operation.Value;
            snapshot[$"Operation_{operation.Key}_Count"] = OperationCount;
            snapshot[$"Operation_{operation.Key}_SuccessCount"] = SuccessCount;
            snapshot[$"Operation_{operation.Key}_FailureCount"] = OperationCount - SuccessCount;
            snapshot[$"Operation_{operation.Key}_SuccessRate"] = OperationCount > 0
                ? (double)SuccessCount / OperationCount * 100
                : 0;
            snapshot[$"Operation_{operation.Key}_AverageDurationMs"] = OperationCount > 0
                ? TotalDuration.TotalMilliseconds / OperationCount
                : 0;
        }

        return snapshot;
    }
}
