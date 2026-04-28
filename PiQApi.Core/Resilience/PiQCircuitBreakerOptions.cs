// PiQApi.Core/Resilience/PiQCircuitBreakerOptions.cs
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Configuration options for circuit breaker policy
/// </summary>
public record PiQCircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets whether the circuit breaker is enabled
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the number of exceptions allowed before breaking
    /// </summary>
    [Range(1, 100)]
    public int ExceptionsAllowedBeforeBreaking { get; init; } = 3;

    /// <summary>
    /// Gets or sets the circuit duration in seconds
    /// </summary>
    [Range(1, 300)]
    public int DurationOfBreakSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the circuit duration as a TimeSpan
    /// </summary>
    public TimeSpan DurationOfBreak => TimeSpan.FromSeconds(DurationOfBreakSeconds);

    /// <summary>
    /// Gets or sets the minimum throughput before circuit decisions are made
    /// </summary>
    [Range(1, 1000)]
    public int MinimumThroughput { get; init; } = 5;

    /// <summary>
    /// Gets or sets the failure threshold percentage (0.0 to 1.0) that triggers the circuit break
    /// </summary>
    [Range(0.01, 1.0)]
    public double FailureThreshold { get; init; } = 0.5;
}