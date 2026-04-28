// CertApi.Core/Resilience/CertPolicyOptions.cs
namespace CertApi.Core.Resilience;

/// <summary>
/// Comprehensive configuration options for resilience policies
/// </summary>
public record CertPolicyOptions
{
    /// <summary>
    /// Gets the retry options
    /// </summary>
    public CertRetryOptions Retry { get; init; } = new CertRetryOptions();

    /// <summary>
    /// Gets the circuit breaker options
    /// </summary>
    public CertCircuitBreakerOptions CircuitBreaker { get; init; } = new CertCircuitBreakerOptions();

    /// <summary>
    /// Gets the timeout options
    /// </summary>
    public CertTimeoutOptions Timeout { get; init; } = new CertTimeoutOptions();

    /// <summary>
    /// Gets the bulkhead options
    /// </summary>
    public CertBulkheadOptions Bulkhead { get; init; } = new CertBulkheadOptions();
}