// CertApi.Core/Resilience/CertPolicyOptionsValidator.cs
using Microsoft.Extensions.Options;

namespace CertApi.Core.Resilience;

/// <summary>
/// Validator for policy options
/// </summary>
public class CertPolicyOptionsValidator : IValidateOptions<CertPolicyOptions>
{
    /// <summary>
    /// Validates policy options
    /// </summary>
    public ValidateOptionsResult Validate(string? name, CertPolicyOptions options)
    {
        // Skip validation for null options
        if (options == null)
            return ValidateOptionsResult.Skip;

        // Cross-component validation
        if (options.CircuitBreaker.Enabled &&
            options.Retry.MaxRetryAttempts > options.CircuitBreaker.ExceptionsAllowedBeforeBreaking)
        {
            return ValidateOptionsResult.Fail(
                "When circuit breaker is enabled, MaxRetryAttempts should not exceed ExceptionsAllowedBeforeBreaking");
        }

        // Validate timeout consistency
        if (options.Bulkhead.ExecutionTimeoutSeconds > options.Timeout.DefaultTimeoutSeconds)
        {
            return ValidateOptionsResult.Fail(
                "Bulkhead execution timeout should not exceed the default timeout");
        }

        return ValidateOptionsResult.Success;
    }
}