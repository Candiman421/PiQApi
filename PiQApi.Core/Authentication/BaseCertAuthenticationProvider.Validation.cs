// PiQApi.Core/Authentication/BaseCertAuthenticationProvider.Validation.cs
using PiQApi.Abstractions.Authentication;

namespace PiQApi.Core.Authentication;

public abstract partial class BaseCertAuthenticationProvider
{
    /// <summary>
    /// Validates a token
    /// </summary>
    public virtual Task<bool> ValidateTokenAsync(ICertTokenInfo token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);
        ct.ThrowIfCancellationRequested();

        string correlationId = CorrelationContext.CorrelationId;
        LogValidatingToken(Logger, correlationId, token.ClientId, null);

        // Track validation in correlation context
        CorrelationContext.AddProperty("TokenValidation", token.ClientId);
        CorrelationContext.AddProperty("TokenType", token.TokenType);
        CorrelationContext.AddProperty("AuthType", token.AuthType.ToString());

        // By default, check if token is expired
        var isValid = !token.IsExpired;

        LogValidationResult(Logger, correlationId, isValid, token.ClientId, null);

        return Task.FromResult(isValid);
    }

    /// <summary>
    /// Invalidates a token
    /// </summary>
    public virtual Task InvalidateTokenAsync(ICertTokenInfo token, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token);
        ct.ThrowIfCancellationRequested();

        string correlationId = CorrelationContext.CorrelationId;
        LogInvalidatingToken(Logger, correlationId, token.ClientId, null);

        // Track invalidation in correlation context
        CorrelationContext.AddProperty("TokenInvalidation", token.ClientId);

        // Default implementation - no action needed for invalidation
        return Task.CompletedTask;
    }
}