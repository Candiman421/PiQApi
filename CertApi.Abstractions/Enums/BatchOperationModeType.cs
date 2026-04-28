// CertApi.Abstractions/Enums/BatchOperationModeType.cs
namespace CertApi.Abstractions.Enums;

/// <summary>
/// Defines how batch operations are processed
/// </summary>
public enum BatchOperationModeType
{
    /// <summary>
    /// Standard operation mode - process batches sequentially
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Parallel operation mode - process batches in parallel
    /// </summary>
    Parallel = 1,

    /// <summary>
    /// Optimized operation mode - dynamically adjust batch size
    /// </summary>
    Optimized = 2
}