// PiQApi.Core/Monitoring/PiQOperationMetricsOptions.cs
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// Configuration options for operation metrics tracking
/// </summary>
public record PiQOperationMetricsOptions
{
    /// <summary>
    /// Determines whether metrics collection is enabled
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Sampling rate for metrics collection (0.0 to 1.0)
    /// </summary>
    [Range(0.0, 1.0)]
    public double SamplingRate { get; init; } = 1.0;

    /// <summary>
    /// Maximum number of custom metrics to track
    /// </summary>
    [Range(0, 1000)]
    public int MaxCustomMetrics { get; init; } = 100;

    /// <summary>
    /// Threshold for long-running operations in milliseconds
    /// </summary>
    [Range(0, int.MaxValue)]
    public long LongRunningOperationThresholdMs { get; init; } = 5000;

    /// <summary>
    /// Maximum number of operations to keep in memory
    /// </summary>
    [Range(0, 10000)]
    public int MaxOperationHistorySize { get; init; } = 1000;

    /// <summary>
    /// Determines whether to log slow operations
    /// </summary>
    public bool LogSlowOperations { get; init; } = true;

    /// <summary>
    /// Creates a new instance with modified sampling rate
    /// </summary>
    /// <param name="samplingRate">New sampling rate (0.0 to 1.0)</param>
    /// <returns>New options instance</returns>
    public PiQOperationMetricsOptions WithSamplingRate(double samplingRate)
    {
        if (samplingRate < 0.0 || samplingRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(samplingRate), "Sampling rate must be between 0.0 and 1.0");

        return this with { SamplingRate = samplingRate };
    }

    /// <summary>
    /// Creates a new instance with metrics collection enabled or disabled
    /// </summary>
    /// <param name="isEnabled">Whether metrics are enabled</param>
    /// <returns>New options instance</returns>
    public PiQOperationMetricsOptions WithEnabled(bool isEnabled)
    {
        return this with { IsEnabled = isEnabled };
    }

    /// <summary>
    /// Creates a new instance with metrics logging enabled or disabled
    /// </summary>
    /// <param name="logSlowOperations">Whether to log slow operations</param>
    /// <returns>New options instance</returns>
    public PiQOperationMetricsOptions WithLogSlowOperations(bool logSlowOperations)
    {
        return this with { LogSlowOperations = logSlowOperations };
    }

    /// <summary>
    /// Creates a new instance with a different slow operation threshold
    /// </summary>
    /// <param name="thresholdMs">Threshold in milliseconds</param>
    /// <returns>New options instance</returns>
    public PiQOperationMetricsOptions WithLongRunningThreshold(long thresholdMs)
    {
        if (thresholdMs < 0)
            throw new ArgumentOutOfRangeException(nameof(thresholdMs), "Threshold must be non-negative");

        return this with { LongRunningOperationThresholdMs = thresholdMs };
    }

    /// <summary>
    /// Validates the current options
    /// </summary>
    /// <exception cref="ValidationException">Thrown when options are invalid</exception>
    public void Validate()
    {
        var validationContext = new ValidationContext(this, serviceProvider: null, items: null);
        Validator.ValidateObject(this, validationContext, validateAllProperties: true);
    }
}