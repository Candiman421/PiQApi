// PiQApi.Core/Authentication/BaseCertAuthenticationProvider.Refresh.cs
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using PiQApi.Core.Core.Models;

namespace PiQApi.Core.Authentication;

public abstract partial class BaseCertAuthenticationProvider
{
    /// <summary>
    /// Refreshes a token
    /// </summary>
    public virtual async Task<IPiQTokenInfo> RefreshTokenAsync(IPiQTokenInfo token, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        var correlationId = CorrelationContext.CorrelationId;
        LogRefreshingToken(Logger, correlationId, token.ClientId, null);

        // Track refresh in correlation context
        CorrelationContext.AddProperty("TokenRefresh", token.ClientId);
        CorrelationContext.AddProperty("TokenRefreshAuthType", token.AuthType.ToString());

        try
        {
            if (token.SourceOptions != null)
            {
                var refreshedToken = await GetTokenAsync(token.SourceOptions, ct).ConfigureAwait(false);

                // Ensure correlation ID is passed to the new token
                if (refreshedToken is PiQTokenInfo certToken &&
                    certToken.CorrelationIdentifier == null &&
                    token is PiQTokenInfo originalCertToken &&
                    originalCertToken.CorrelationIdentifier != null)
                {
                    // Copy correlation ID if possible
                    var builder = PiQTokenInfo.CreateBuilder()
                        .WithToken(certToken.AccessToken)
                        .WithExpiresAt(certToken.ExpiresOn)
                        .WithScopes(certToken.Scopes as IReadOnlyList<string> ?? certToken.Scopes.ToList())
                        .WithTenantId(certToken.TenantId)
                        .WithClientId(certToken.ClientId)
                        .WithAuthenticationMethodType(certToken.AuthType)
                        .WithCorrelationId(originalCertToken.CorrelationIdentifier);

                    return builder.Build();
                }

                return refreshedToken;
            }

            LogCannotRefresh(Logger, correlationId, token.ClientId, null);

            // Create a token with minimal info indicating failure
            // This maintains existing behavior but in a result-oriented way
            var expiredToken = new PiQTokenInfo(
                token.AccessToken,
                TimeProvider.UtcNow.AddMinutes(-1), // Explicitly set as expired
                token.Scopes,
                token.TenantId,
                token.ClientId,
                token.AuthType,
                new Dictionary<string, string> {
                    { "RefreshError", "Cannot refresh token without source options" },
                    { "CorrelationId", correlationId }
                }
            );

            return expiredToken;
        }
        catch (Exception ex)
        {
            // Add correlation ID to exception data
            if (!ex.Data.Contains("CorrelationId"))
            {
                ex.Data["CorrelationId"] = correlationId;
                ex.Data["ClientId"] = token.ClientId;
                ex.Data["AuthType"] = token.AuthType.ToString();
            }

            throw;
        }
    }

    /// <summary>
    /// Creates a token with correlation ID
    /// </summary>
    protected static PiQTokenInfo CreateTokenWithCorrelation(
        string accessToken,
        DateTime expiresOn,
        IEnumerable<string> scopes,
        string tenantId,
        string clientId,
        AuthenticationMethodType authType,
        IPiQCorrelationContext correlationContext)
    {
        ArgumentNullException.ThrowIfNull(correlationContext);

        // Use the constructor that takes just a string ID
        var correlationId = new PiQCorrelationId(correlationContext.CorrelationId);

        return new PiQTokenInfo(
            accessToken,
            expiresOn,
            scopes,
            tenantId,
            clientId,
            authType,
            correlationId);
    }
}