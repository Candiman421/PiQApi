// CertApi.Core/Authentication/BaseCertAuthenticationProvider.Refresh.cs
using CertApi.Abstractions.Authentication;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;
using CertApi.Core.Core.Models;

namespace CertApi.Core.Authentication;

public abstract partial class BaseCertAuthenticationProvider
{
    /// <summary>
    /// Refreshes a token
    /// </summary>
    public virtual async Task<ICertTokenInfo> RefreshTokenAsync(ICertTokenInfo token, CancellationToken ct = default)
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
                if (refreshedToken is CertTokenInfo certToken &&
                    certToken.CorrelationIdentifier == null &&
                    token is CertTokenInfo originalCertToken &&
                    originalCertToken.CorrelationIdentifier != null)
                {
                    // Copy correlation ID if possible
                    var builder = CertTokenInfo.CreateBuilder()
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
            var expiredToken = new CertTokenInfo(
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
    protected static CertTokenInfo CreateTokenWithCorrelation(
        string accessToken,
        DateTime expiresOn,
        IEnumerable<string> scopes,
        string tenantId,
        string clientId,
        AuthenticationMethodType authType,
        ICertCorrelationContext correlationContext)
    {
        ArgumentNullException.ThrowIfNull(correlationContext);

        // Use the constructor that takes just a string ID
        var correlationId = new CertCorrelationId(correlationContext.CorrelationId);

        return new CertTokenInfo(
            accessToken,
            expiresOn,
            scopes,
            tenantId,
            clientId,
            authType,
            correlationId);
    }
}