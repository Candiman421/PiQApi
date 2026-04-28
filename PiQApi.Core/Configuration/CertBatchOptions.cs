// PiQApi.Core/Configuration/CertBatchOptions.cs
using System.ComponentModel.DataAnnotations;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Core.Configuration;

/// <summary>
/// Configuration options for batch operations
/// </summary>
public record CertBatchOptions
{
    /// <summary>
    /// Gets or sets the maximum batch size
    /// </summary>
    [Range(1, 1000)]
    public int MaxBatchSize { get; init; } = 100;

    /// <summary>
    /// Gets or sets whether batch optimization is enabled
    /// </summary>
    public bool EnableBatchOptimization { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum concurrent batches
    /// </summary>
    [Range(1, 100)]
    public int MaxConcurrentBatches { get; init; } = 5;

    /// <summary>
    /// Gets or sets the batch timeout in seconds
    /// </summary>
    [Range(1, 3600)]
    public int BatchTimeoutSeconds { get; init; } = 300;

    /// <summary>
    /// Gets or sets the operation mode
    /// </summary>
    public BatchOperationModeType OperationMode { get; init; } = BatchOperationModeType.Standard;
}