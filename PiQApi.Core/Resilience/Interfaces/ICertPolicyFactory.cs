// PiQApi.Core/Resilience/Interfaces/ICertPolicyFactory.cs
using PiQApi.Abstractions.Enums;
using Polly;

namespace PiQApi.Core.Resilience.Interfaces;

/// <summary>
/// Factory interface for creating resilience policies
/// </summary>
public interface ICertPolicyFactory
{
    /// <summary>
    /// Creates a policy for the specified policy type
    /// </summary>
    IAsyncPolicy<T> CreatePolicy<T>(ResiliencePolicyType policyType);

    /// <summary>
    /// Creates a default policy
    /// </summary>
    IAsyncPolicy<T> CreateDefaultPolicy<T>();

    /// <summary>
    /// Creates an authentication policy
    /// </summary>
    IAsyncPolicy<T> CreateAuthenticationPolicy<T>();

    /// <summary>
    /// Creates a timeout policy
    /// </summary>
    IAsyncPolicy<T> CreateTimeoutPolicy<T>(TimeSpan timeout);

    /// <summary>
    /// Creates a bulkhead policy
    /// </summary>
    IAsyncPolicy<T> CreateBulkheadPolicy<T>();

    /// <summary>
    /// Creates a circuit breaker policy
    /// </summary>
    IAsyncPolicy<T> CreateCircuitBreakerPolicy<T>();

    /// <summary>
    /// Determines whether an exception is transient
    /// </summary>
    bool IsTransientException(Exception ex);
}
