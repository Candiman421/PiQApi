// CertApi.Core/Configuration/CertConnectionOptionsValidator.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CertApi.Core.Configuration;

/// <summary>
/// Validator for connection options
/// </summary>
public class CertConnectionOptionsValidator : IValidateOptions<CertConnectionOptions>
{
    private readonly ILogger<CertConnectionOptionsValidator> _logger;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, Exception?> LogValidationFailure =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, "ConnectionOptionsValidation"),
            "Connection options validation failed: {ValidationMessage}");

    /// <summary>
    /// Creates a new instance of CertConnectionOptionsValidator
    /// </summary>
    public CertConnectionOptionsValidator(ILogger<CertConnectionOptionsValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates connection options
    /// </summary>
    public ValidateOptionsResult Validate(string? name, CertConnectionOptions options)
    {
        // Skip validation for null options
        if (options == null)
            return ValidateOptionsResult.Skip;

        // Validate endpoint
        if (options.Endpoint == null)
        {
            LogValidationFailure(_logger, "Endpoint is required", null);
            return ValidateOptionsResult.Fail("Endpoint is required");
        }

        // Validate endpoint name
        if (string.IsNullOrWhiteSpace(options.Endpoint.EndpointName))
        {
            LogValidationFailure(_logger, "Endpoint name is required", null);
            return ValidateOptionsResult.Fail("Endpoint name is required");
        }

        // Validate pool size
        if (options.PoolSize <= 0)
        {
            var message = $"Pool size must be positive, got {options.PoolSize}";
            LogValidationFailure(_logger, message, null);
            return ValidateOptionsResult.Fail(message);
        }

        // Validate timeout values
        if (options.IdleTimeoutSeconds <= 0)
        {
            var message = $"Idle timeout must be positive, got {options.IdleTimeoutSeconds}";
            LogValidationFailure(_logger, message, null);
            return ValidateOptionsResult.Fail(message);
        }

        if (options.LifetimeSeconds <= 0)
        {
            var message = $"Lifetime must be positive, got {options.LifetimeSeconds}";
            LogValidationFailure(_logger, message, null);
            return ValidateOptionsResult.Fail(message);
        }

        return ValidateOptionsResult.Success;
    }
}