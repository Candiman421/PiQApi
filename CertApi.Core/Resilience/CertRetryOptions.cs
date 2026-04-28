// CertApi.Core/Resilience/CertRetryOptions.cs
using System.ComponentModel.DataAnnotations;

namespace CertApi.Core.Resilience;

/// <summary>
/// Configuration options for retry policy
/// </summary>
public record CertRetryOptions
{
    /// <summary>
    /// Gets or sets the maximum retry attempts
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets or sets the maximum authentication retry attempts
    /// </summary>
    [Range(0, 5)]
    public int MaxAuthRetryAttempts { get; init; } = 2;

    /// <summary>
    /// Gets or sets the retry delay in milliseconds
    /// </summary>
    [Range(100, 30000)]
    public int RetryDelayMs { get; init; } = 1000;

    /// <summary>
    /// Gets or sets the maximum retry delay in milliseconds
    /// </summary>
    [Range(1000, 60000)]
    public int MaxRetryDelayMs { get; init; } = 30000;

    /// <summary>
    /// Gets or sets the initial backoff in seconds
    /// </summary>
    [Range(0.1, 30)]
    public double InitialBackoffSeconds { get; init; } = 1;

    /// <summary>
    /// Gets or sets the maximum backoff in seconds
    /// </summary>
    [Range(1, 300)]
    public double MaxBackoffSeconds { get; init; } = 30;

    /// <summary>
    /// Gets or sets the backoff multiplier
    /// </summary>
    [Range(1, 10)]
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Gets or sets whether to use jitter in backoff calculations
    /// </summary>
    public bool UseJitter { get; init; } = true;

    /// <summary>
    /// Validates that max retry delay is greater than retry delay
    /// </summary>
    public bool IsValidRetryDelay => RetryDelayMs <= MaxRetryDelayMs;

    /// <summary>
    /// Validates that max backoff is greater than initial backoff
    /// </summary>
    public bool IsValidBackoff => InitialBackoffSeconds <= MaxBackoffSeconds;
}