// PiQApi.Core/Resilience/PiQPolicyOptions.cs
namespace PiQApi.Core.Resilience;

/// <summary>
/// Comprehensive configuration options for resilience policies
/// </summary>
public record PiQPolicyOptions
{
    /// <summary>
    /// Gets the retry options
    /// </summary>
    public PiQRetryOptions Retry { get; init; } = new PiQRetryOptions();

    /// <summary>
    /// Gets the circuit breaker options
    /// </summary>
    public PiQCircuitBreakerOptions CircuitBreaker { get; init; } = new PiQCircuitBreakerOptions();

    /// <summary>
    /// Gets the timeout options
    /// </summary>
    public PiQTimeoutOptions Timeout { get; init; } = new PiQTimeoutOptions();

    /// <summary>
    /// Gets the bulkhead options
    /// </summary>
    public PiQBulkheadOptions Bulkhead { get; init; } = new PiQBulkheadOptions();
}