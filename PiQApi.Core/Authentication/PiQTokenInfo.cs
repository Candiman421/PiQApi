// PiQApi.Core/Authentication/PiQTokenInfo.cs
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Enums;
using PiQApi.Core.Core.Models;

namespace PiQApi.Core.Authentication;

/// <summary>
/// Enhanced token information with additional functionality
/// </summary>
public class PiQTokenInfo : IPiQTokenInfo
{
    private const int RefreshThresholdMinutes = 5;
    private readonly Dictionary<string, string> _claims = new();

    /// <summary>
    /// Gets the access token string
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Gets the time when the token expires
    /// </summary>
    public DateTime ExpiresOn { get; }

    /// <summary>
    /// Gets the token scopes
    /// </summary>
    public IEnumerable<string> Scopes { get; }

    /// <summary>
    /// Gets the token type (e.g. Bearer)
    /// </summary>
    public string TokenType { get; }

    /// <summary>
    /// Gets the tenant ID
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// Gets the client ID
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// Gets the authentication type
    /// </summary>
    public AuthenticationMethodType AuthType { get; }

    /// <summary>
    /// Gets whether the token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresOn;

    /// <summary>
    /// Gets the original authentication options used to create this token
    /// </summary>
    public IPiQAuthenticationOptions? SourceOptions { get; }

    /// <summary>
    /// Gets the time when the token was created
    /// </summary>
    public DateTime CreatedOn { get; }

    /// <summary>
    /// Gets whether the token should be refreshed (within 5 minutes of expiration)
    /// </summary>
    public bool ShouldRefresh => DateTime.UtcNow.AddMinutes(RefreshThresholdMinutes) > ExpiresOn;

    /// <summary>
    /// Gets the correlation ID associated with this token
    /// </summary>
    public PiQCorrelationId? CorrelationIdentifier { get; }

    /// <summary>
    /// Creates a new PiQTokenInfo instance with the specified properties
    /// </summary>
    public PiQTokenInfo(
        string accessToken,
        DateTime expiresOn,
        IEnumerable<string> scopes,
        string tenantId,
        string clientId,
        AuthenticationMethodType authType,
        PiQCorrelationId? correlationId = null)
    {
        AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        ExpiresOn = expiresOn;
        Scopes = scopes ?? Array.Empty<string>();
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        AuthType = authType;
        TokenType = "Bearer";
        CreatedOn = DateTime.UtcNow;
        CorrelationIdentifier = correlationId;
    }

    /// <summary>
    /// Creates a new PiQTokenInfo instance with the specified properties and claims
    /// </summary>
    public PiQTokenInfo(
        string accessToken,
        DateTime expiresOn,
        IEnumerable<string> scopes,
        string tenantId,
        string clientId,
        AuthenticationMethodType authType,
        IDictionary<string, string> claims,
        IPiQAuthenticationOptions? sourceOptions = null,
        PiQCorrelationId? correlationId = null)
        : this(accessToken, expiresOn, scopes, tenantId, clientId, authType, correlationId)
    {
        ArgumentNullException.ThrowIfNull(claims);

        foreach (var claim in claims)
        {
            _claims[claim.Key] = claim.Value;
        }

        SourceOptions = sourceOptions;
    }

    /// <summary>
    /// Creates a new PiQTokenInfo instance from a PiQTokenInfoBuilder
    /// </summary>
    internal PiQTokenInfo(PiQTokenInfoBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Validate required fields
        if (string.IsNullOrEmpty(builder.AccessToken))
        {
            throw new ArgumentException("Access token is required", nameof(builder));
        }

        if (string.IsNullOrEmpty(builder.TenantId))
        {
            throw new ArgumentException("Tenant ID is required", nameof(builder));
        }

        if (string.IsNullOrEmpty(builder.ClientId))
        {
            throw new ArgumentException("Client ID is required", nameof(builder));
        }

        AccessToken = builder.AccessToken;
        ExpiresOn = builder.ExpiresOn;
        Scopes = builder.Scopes ?? Array.Empty<string>();
        TenantId = builder.TenantId;
        ClientId = builder.ClientId;
        AuthType = builder.AuthType;
        TokenType = builder.TokenType;
        CreatedOn = DateTime.UtcNow;
        SourceOptions = builder.AuthenticationOptions;
        CorrelationIdentifier = builder.CorrelationIdentifier;

        // Add claims from builder
        foreach (var claim in builder.Claims)
        {
            _claims[claim.Key] = claim.Value;
        }
    }

    /// <summary>
    /// Creates a new instance with updated token information
    /// </summary>
    public PiQTokenInfo WithUpdatedToken(string accessToken, DateTime expiresOn, IEnumerable<string> scopes)
    {
        ArgumentNullException.ThrowIfNull(accessToken);

        return new PiQTokenInfo(
            accessToken,
            expiresOn,
            scopes ?? Scopes,
            TenantId,
            ClientId,
            AuthType,
            new Dictionary<string, string>(_claims),
            SourceOptions,
            CorrelationIdentifier);
    }

    /// <summary>
    /// Creates a new instance with an additional claim
    /// </summary>
    public PiQTokenInfo WithClaim(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);

        var claims = new Dictionary<string, string>(_claims)
        {
            [key] = value
        };

        return new PiQTokenInfo(
            AccessToken,
            ExpiresOn,
            Scopes,
            TenantId,
            ClientId,
            AuthType,
            claims,
            SourceOptions,
            CorrelationIdentifier);
    }

    /// <summary>
    /// Checks if the token has a specific scope
    /// </summary>
    public bool HasScope(string scope)
    {
        ArgumentException.ThrowIfNullOrEmpty(scope);
        return Scopes.Any(s => string.Equals(s, scope, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the token has a specific claim
    /// </summary>
    public bool HasClaim(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _claims.ContainsKey(key);
    }

    /// <summary>
    /// Gets the value of a claim
    /// </summary>
    public string? GetClaimValue(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _claims.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Creates a new PiQTokenInfoBuilder
    /// </summary>
    public static PiQTokenInfoBuilder CreateBuilder()
    {
        return new PiQTokenInfoBuilder();
    }
}
