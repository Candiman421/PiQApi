// PiQApi.Core/Resilience/PiQBulkheadOptions.cs
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Configuration options for bulkhead policy
/// </summary>
public record PiQBulkheadOptions
{
    /// <summary>
    /// Gets or sets the maximum concurrent operations
    /// </summary>
    [Range(1, 1000)]
    public int MaxConcurrentOperations { get; init; } = 100;

    /// <summary>
    /// Gets or sets the maximum queue size
    /// </summary>
    [Range(0, 1000)]
    public int MaxQueueSize { get; init; } = 50;

    /// <summary>
    /// Gets or sets the execution timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int ExecutionTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets or sets the maximum parallelization
    /// This is an alias for MaxConcurrentOperations for backward compatibility
    /// </summary>
    public int MaxParallelization => MaxConcurrentOperations;

    /// <summary>
    /// Gets or sets the maximum queued items
    /// This is an alias for MaxQueueSize for backward compatibility
    /// </summary>
    public int MaxQueuedItems => MaxQueueSize;

    /// <summary>
    /// Gets the execution timeout as a TimeSpan
    /// </summary>
    public TimeSpan ExecutionTimeout => TimeSpan.FromSeconds(ExecutionTimeoutSeconds);
}