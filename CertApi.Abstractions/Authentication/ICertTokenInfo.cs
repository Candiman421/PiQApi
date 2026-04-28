// CertApi.Abstractions/Authentication/ICertTokenInfo.cs
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Authentication;

/// <summary>
/// Represents authentication token information
/// </summary>
public interface ICertTokenInfo
{
    /// <summary>
    /// Gets the access token string
    /// </summary>
    string AccessToken { get; }

    /// <summary>
    /// Gets the time when the token expires
    /// </summary>
    DateTime ExpiresOn { get; }

    /// <summary>
    /// Gets the token scopes
    /// </summary>
    IEnumerable<string> Scopes { get; }

    /// <summary>
    /// Gets the token type (e.g. Bearer)
    /// </summary>
    string TokenType { get; }

    /// <summary>
    /// Gets the tenant ID
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Gets the client ID
    /// </summary>
    string ClientId { get; }

    /// <summary>
    /// Gets the authentication type
    /// </summary>
    AuthenticationMethodType AuthType { get; }

    /// <summary>
    /// Gets whether the token is expired
    /// </summary>
    bool IsExpired { get; }

    /// <summary>
    /// Gets the original authentication options used to create this token
    /// </summary>
    ICertAuthenticationOptions? SourceOptions { get; }

    /// <summary>
    /// Checks if the token has a specific scope
    /// </summary>
    bool HasScope(string scope);

    /// <summary>
    /// Checks if the token has a specific claim
    /// </summary>
    bool HasClaim(string key);

    /// <summary>
    /// Gets the value of a claim
    /// </summary>
    string? GetClaimValue(string key);
}
