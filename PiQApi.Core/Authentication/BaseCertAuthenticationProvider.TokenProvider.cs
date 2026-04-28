// PiQApi.Core/Authentication/BaseCertAuthenticationProvider.TokenProvider.cs
using PiQApi.Abstractions.Authentication;

namespace PiQApi.Core.Authentication;

public abstract partial class BaseCertAuthenticationProvider
{
    /// <summary>
    /// Gets an access token string
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token string</returns>
    public virtual async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Track operation in correlation context
        CorrelationContext.AddProperty("AuthOperation", "GetAccessToken");
        CorrelationContext.AddProperty("AuthType", AuthType.ToString());

        try
        {
            // Get a token via the structured interface and return just the token string
            var tokenInfo = await GetTokenAsync(GetDefaultOptions(), cancellationToken).ConfigureAwait(false);
            return tokenInfo.AccessToken;
        }
        catch (Exception ex)
        {
            // Ensure correlation ID is added to exception data
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationContext.CorrelationId))
            {
                ex.Data["CorrelationId"] = CorrelationContext.CorrelationId;
                ex.Data["AuthType"] = AuthType.ToString();
                ex.Data["Operation"] = "GetAccessToken";
            }

            throw;
        }
    }

    /// <summary>
    /// Refreshes an access token string
    /// </summary>
    /// <param name="accessToken">Current access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refreshed access token string</returns>
    public virtual async Task<string> RefreshTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        // Track operation in correlation context
        string correlationId = CorrelationContext.CorrelationId;
        CorrelationContext.AddProperty("AuthOperation", "RefreshAccessToken");
        CorrelationContext.AddProperty("AuthType", AuthType.ToString());

        // Create a minimal token info for the refresh operation
        var tokenInfo = CreateMinimalTokenInfo(accessToken);

        LogRefreshingToken(Logger, correlationId, tokenInfo.ClientId, null);

        try
        {
            // Refresh via the structured interface and return just the token string
            var refreshedToken = await RefreshTokenAsync(tokenInfo, cancellationToken).ConfigureAwait(false);
            return refreshedToken.AccessToken;
        }
        catch (Exception ex)
        {
            // Ensure correlation ID is added to exception data
            if (!ex.Data.Contains("CorrelationId"))
            {
                ex.Data["CorrelationId"] = correlationId;
                ex.Data["AuthType"] = AuthType.ToString();
                ex.Data["ClientId"] = tokenInfo.ClientId;
            }

            throw;
        }
    }

    /// <summary>
    /// Create minimal token info from an access token string
    /// </summary>
    protected virtual ICertTokenInfo CreateMinimalTokenInfo(string accessToken)
    {
        // This is a default implementation - derived classes should override this
        // to provide proper token info creation logic
        return new CertTokenInfo(
            accessToken,
            DateTime.UtcNow.AddHours(1), // Default 1-hour expiration
            Array.Empty<string>(),       // Empty scopes
            string.Empty,                // No tenant ID
            "default-client",            // Default client ID
            AuthType,                    // Authentication type from derived class
            null);                       // No correlation ID
    }

    /// <summary>
    /// Gets default authentication options
    /// </summary>
    protected abstract ICertAuthenticationOptions GetDefaultOptions();
}
