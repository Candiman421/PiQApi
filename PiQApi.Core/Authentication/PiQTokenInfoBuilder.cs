// PiQApi.Core/Authentication/PiQTokenInfoBuilder.cs
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Enums;
using PiQApi.Core.Core.Models;

namespace PiQApi.Core.Authentication;

/// <summary>
/// Builder for creating token information instances
/// </summary>
public class PiQTokenInfoBuilder
{
    // Expose these as read-only properties for the PiQTokenInfo constructor
    internal string AccessToken { get; private set; } = string.Empty;
    internal string TokenType { get; private set; } = "Bearer";
    internal DateTime ExpiresOn { get; private set; } = DateTime.UtcNow.AddHours(1);
    internal IReadOnlyList<string> Scopes { get; private set; } = Array.Empty<string>();
    internal string TenantId { get; private set; } = string.Empty;
    internal string ClientId { get; private set; } = string.Empty;
    internal AuthenticationMethodType AuthType { get; private set; } = AuthenticationMethodType.OAuth;
    internal Dictionary<string, string> Claims { get; } = new();
    internal IPiQAuthenticationOptions? AuthenticationOptions { get; private set; }
    internal PiQCorrelationId? CorrelationIdentifier { get; private set; }

    /// <summary>
    /// Sets the access token
    /// </summary>
    /// <param name="token">The access token</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when token is null</exception>
    public PiQTokenInfoBuilder WithToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        AccessToken = token;
        return this;
    }

    /// <summary>
    /// Sets the token type
    /// </summary>
    /// <param name="tokenType">The token type (e.g., "Bearer")</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when tokenType is null</exception>
    public PiQTokenInfoBuilder WithTokenType(string tokenType)
    {
        ArgumentNullException.ThrowIfNull(tokenType);
        TokenType = tokenType;
        return this;
    }

    /// <summary>
    /// Sets the token expiration time
    /// </summary>
    /// <param name="expiresAt">The expiration DateTime</param>
    /// <returns>The builder instance for method chaining</returns>
    public PiQTokenInfoBuilder WithExpiresAt(DateTime expiresAt)
    {
        ExpiresOn = expiresAt;
        return this;
    }

    /// <summary>
    /// Sets the token expiration based on seconds from now
    /// </summary>
    /// <param name="expiresInSeconds">Seconds until token expiration</param>
    /// <returns>The builder instance for method chaining</returns>
    public PiQTokenInfoBuilder WithExpiresInSeconds(int expiresInSeconds)
    {
        ExpiresOn = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        return this;
    }

    /// <summary>
    /// Sets the refresh token (not currently stored)
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when refreshToken is null</exception>
    public PiQTokenInfoBuilder WithRefreshToken(string refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        // Not storing refresh token as it's not used in the PiQTokenInfo
        return this;
    }

    /// <summary>
    /// Sets the authentication method type
    /// </summary>
    /// <param name="authType">The authentication method type</param>
    /// <returns>The builder instance for method chaining</returns>
    public PiQTokenInfoBuilder WithAuthenticationMethodType(AuthenticationMethodType authType)
    {
        AuthType = authType;
        return this;
    }

    /// <summary>
    /// Sets the tenant ID
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when tenantId is null</exception>
    public PiQTokenInfoBuilder WithTenantId(string tenantId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        TenantId = tenantId;
        return this;
    }

    /// <summary>
    /// Sets the client ID
    /// </summary>
    /// <param name="clientId">The client ID</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when clientId is null</exception>
    public PiQTokenInfoBuilder WithClientId(string clientId)
    {
        ArgumentNullException.ThrowIfNull(clientId);
        ClientId = clientId;
        return this;
    }

    /// <summary>
    /// Sets the token scopes
    /// </summary>
    /// <param name="scopes">The collection of scopes</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when scopes is null</exception>
    public PiQTokenInfoBuilder WithScopes(IReadOnlyList<string> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        Scopes = scopes;
        return this;
    }

    /// <summary>
    /// Sets the authentication options
    /// </summary>
    /// <param name="options">The authentication options</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public PiQTokenInfoBuilder WithAuthenticationOptions(IPiQAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        AuthenticationOptions = options;
        return this;
    }

    /// <summary>
    /// Adds a claim to the token
    /// </summary>
    /// <param name="key">The claim key</param>
    /// <param name="value">The claim value</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when key or value is null or empty</exception>
    public PiQTokenInfoBuilder WithClaim(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
        Claims[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple claims to the token
    /// </summary>
    /// <param name="claims">Dictionary of claims</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when claims is null</exception>
    public PiQTokenInfoBuilder WithClaims(Dictionary<string, string> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        foreach (var claim in claims)
        {
            if (!string.IsNullOrEmpty(claim.Key) && !string.IsNullOrEmpty(claim.Value))
            {
                Claims[claim.Key] = claim.Value;
            }
        }

        return this;
    }

    /// <summary>
    /// Sets the correlation ID
    /// </summary>
    /// <param name="correlationId">The correlation ID</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when correlationId is null</exception>
    public PiQTokenInfoBuilder WithCorrelationId(PiQCorrelationId correlationId)
    {
        ArgumentNullException.ThrowIfNull(correlationId);
        CorrelationIdentifier = correlationId;
        return this;
    }

    /// <summary>
    /// Builds the token information object with the configured properties
    /// </summary>
    /// <returns>A new token information instance</returns>
    public PiQTokenInfo Build()
    {
        return new PiQTokenInfo(this);
    }
}
