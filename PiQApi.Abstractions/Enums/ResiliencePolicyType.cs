// PiQApi.Abstractions/Enums/ResiliencePolicyType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines types of resilience policies
/// </summary>
public enum ResiliencePolicyType
{
    /// <summary>
    /// Default policy (retry with exponential backoff)
    /// </summary>
    Default = 0,

    /// <summary>
    /// Authentication-specific policy
    /// </summary>
    Authentication = 1,

    /// <summary>
    /// No resilience policy
    /// </summary>
    None = 2,

    /// <summary>
    /// Timeout policy
    /// </summary>
    Timeout = 3,

    /// <summary>
    /// Circuit breaker policy
    /// </summary>
    CircuitBreaker = 4,

    /// <summary>
    /// Bulkhead policy for concurrency control
    /// </summary>
    Bulkhead = 5,

    /// <summary>
    /// Combined policy (default + circuit breaker)
    /// </summary>
    Combined = 6,

    /// <summary>
    /// Retry policy
    /// </summary>
    Retry = 7,

    /// <summary>
    /// Custom policy defined by application
    /// </summary>
    Custom = 99
}