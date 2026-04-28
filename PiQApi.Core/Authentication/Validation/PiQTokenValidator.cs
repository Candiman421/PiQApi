// PiQApi.Core/Authentication/Validation/PiQTokenValidator.cs
using System.Collections.Immutable;
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Core.Core.Models;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Authentication.Validation;

/// <summary>
/// Validator for authentication tokens
/// </summary>
public class PiQTokenValidator : IPiQTokenValidator
{
    private readonly ILogger<PiQTokenValidator> _logger;
    private readonly IPiQValidationProcessor _validationProcessor;
    private readonly IPiQCorrelationContext _correlationContext;

    // LoggerMessage delegate for improved performance
    private static readonly Action<ILogger, string, string, string, Exception?> LogValidatingToken =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(ValidateTokenAsync)),
            "[{CorrelationId}] Validating token for client {ClientId} of type {AuthType}");

    private static readonly Action<ILogger, string, string, bool, Exception?> LogValidationResult =
        LoggerMessage.Define<string, string, bool>(
            LogLevel.Debug,
            new EventId(2, nameof(ValidateTokenAsync)),
            "[{CorrelationId}] Token validation for client {ClientId} result: {IsValid}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogInvalidatingToken =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Debug,
            new EventId(3, nameof(InvalidateTokenAsync)),
            "[{CorrelationId}] Invalidating token for client {ClientId} of type {AuthType}");

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQTokenValidator"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="validationProcessor">The validation processor</param>
    /// <param name="correlationContext">The correlation context</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public PiQTokenValidator(
        ILogger<PiQTokenValidator> logger,
        IPiQValidationProcessor validationProcessor,
        IPiQCorrelationContext correlationContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validationProcessor = validationProcessor ?? throw new ArgumentNullException(nameof(validationProcessor));
        _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
    }

    /// <summary>
    /// Validates a token asynchronously
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if token is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when token is null</exception>
    public async Task<bool> ValidateTokenAsync(IPiQTokenInfo token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Track validation in correlation context
        _correlationContext.AddProperty("ValidationOperation", "TokenValidation");
        _correlationContext.AddProperty("TokenClientId", token.ClientId);
        _correlationContext.AddProperty("TokenAuthType", token.AuthType.ToString());

        LogValidatingToken(_logger, _correlationContext.CorrelationId, token.ClientId, token.AuthType.ToString(), null);

        // Convert interface to concrete type if possible
        if (token is PiQTokenInfo concreteToken)
        {
            var result = await ValidateTokenAsync(concreteToken, ct).ConfigureAwait(false);

            // Add validation result to correlation context
            _correlationContext.AddProperty("ValidationResult", result.IsValid);
            LogValidationResult(_logger, _correlationContext.CorrelationId, token.ClientId, result.IsValid, null);

            return result.IsValid;
        }

        // Create a correlation ID from the current context
        var correlationId = new PiQCorrelationId(_correlationContext.CorrelationId, PiQTimeProviderFactory.Current);

        // Use the constructor that accepts IPiQCorrelationId and CancellationToken
        var context = new PiQValidationContext(
            ValidationModeType.Standard,
            0,
            10,
            false,
            correlationId, // Pass IPiQCorrelationId directly
            ImmutableDictionary<string, object>.Empty,
            ct); // Pass CancellationToken directly

        var contextWithValues = context
            .WithContextValue("ClientId", token.ClientId)
            .WithContextValue("AuthType", token.AuthType.ToString());

        var validationResult = await _validationProcessor.ValidateAsync(token, contextWithValues, ct).ConfigureAwait(false);

        // Add validation result to correlation context
        _correlationContext.AddProperty("ValidationResult", validationResult.IsValid);
        LogValidationResult(_logger, _correlationContext.CorrelationId, token.ClientId, validationResult.IsValid, null);

        return validationResult.IsValid;
    }

    /// <summary>
    /// Invalidates a token
    /// </summary>
    /// <param name="token">Token to invalidate</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="ArgumentNullException">Thrown when token is null</exception>
    public Task InvalidateTokenAsync(IPiQTokenInfo token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Track invalidation in correlation context
        _correlationContext.AddProperty("ValidationOperation", "TokenInvalidation");
        _correlationContext.AddProperty("TokenClientId", token.ClientId);
        _correlationContext.AddProperty("TokenAuthType", token.AuthType.ToString());

        LogInvalidatingToken(_logger, _correlationContext.CorrelationId, token.ClientId, token.AuthType.ToString(), null);

        // Default implementation - no action needed for invalidation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates a token and returns a validation result
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result indicating if the token is valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when token is null</exception>
    public async Task<IPiQValidationResult> ValidateTokenAsync(
        PiQTokenInfo token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Track validation in correlation context
        _correlationContext.AddProperty("ValidationOperation", "DetailedTokenValidation");
        _correlationContext.AddProperty("TokenClientId", token.ClientId);
        _correlationContext.AddProperty("TokenAuthType", token.AuthType.ToString());

        LogValidatingToken(_logger, _correlationContext.CorrelationId, token.ClientId, token.AuthType.ToString(), null);

        // Create a correlation ID from the current context
        var correlationId = new PiQCorrelationId(_correlationContext.CorrelationId, PiQTimeProviderFactory.Current);

        // Use the constructor that accepts IPiQCorrelationId and CancellationToken
        var context = new PiQValidationContext(
            ValidationModeType.Standard,
            0,
            10,
            false,
            correlationId, // Pass IPiQCorrelationId directly
            ImmutableDictionary<string, object>.Empty,
            cancellationToken); // Pass CancellationToken directly

        // Add additional context information
        var contextWithValues = context
            .WithContextValue("ClientId", token.ClientId)
            .WithContextValue("AuthType", token.AuthType.ToString());

        // Process validation by passing both token and context
        var result = await _validationProcessor.ValidateAsync(token, contextWithValues, cancellationToken).ConfigureAwait(false);

        // Add validation result to correlation context
        _correlationContext.AddProperty("ValidationResultIsValid", result.IsValid);
        if (!result.IsValid && result.Errors.Count != 0)
        {
            _correlationContext.AddProperty("ValidationErrors", string.Join(", ", result.Errors.Select(e => e.Message)));
        }

        return result;
    }

    /// <summary>
    /// Checks if a token is valid asynchronously
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the token is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when token is null</exception>
    public async Task<bool> IsValidAsync(PiQTokenInfo token, CancellationToken cancellationToken = default)
    {
        var result = await ValidateTokenAsync(token, cancellationToken).ConfigureAwait(false);
        return result.IsValid;
    }

    /// <summary>
    /// Validates a token synchronously
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>Validation result indicating if the token is valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when token is null</exception>
    public IPiQValidationResult ValidateToken(PiQTokenInfo token)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Track validation in correlation context
        _correlationContext.AddProperty("ValidationOperation", "SyncTokenValidation");
        _correlationContext.AddProperty("TokenClientId", token.ClientId);
        _correlationContext.AddProperty("TokenAuthType", token.AuthType.ToString());

        LogValidatingToken(_logger, _correlationContext.CorrelationId, token.ClientId, token.AuthType.ToString(), null);

        // Create a correlation ID from the current context
        var correlationId = new PiQCorrelationId(_correlationContext.CorrelationId, PiQTimeProviderFactory.Current);

        // Use the constructor that accepts IPiQCorrelationId and CancellationToken
        var context = new PiQValidationContext(
            ValidationModeType.Standard,
            0,
            10,
            false,
            correlationId, // Pass IPiQCorrelationId directly
            ImmutableDictionary<string, object>.Empty,
            CancellationToken.None); // Pass default CancellationToken

        // Add additional context information
        var contextWithValues = context
            .WithContextValue("ClientId", token.ClientId)
            .WithContextValue("AuthType", token.AuthType.ToString());

        var result = _validationProcessor.Validate(token, contextWithValues);

        // Add validation result to correlation context
        _correlationContext.AddProperty("ValidationResultIsValid", result.IsValid);
        if (!result.IsValid && result.Errors.Count != 0)
        {
            _correlationContext.AddProperty("ValidationErrors", string.Join(", ", result.Errors.Select(e => e.Message)));
        }

        return result;
    }

    /// <summary>
    /// Checks if a token is valid synchronously
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>True if the token is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when token is null</exception>
    public bool IsValid(PiQTokenInfo token)
    {
        return ValidateToken(token).IsValid;
    }
}