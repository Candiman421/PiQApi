// CertApi.Core/Authentication/CertAuthenticationResult.cs

using CertApi.Abstractions.Authentication;

namespace CertApi.Core.Authentication;

/// <summary>
/// Authentication result implementation
/// </summary>
public class CertAuthenticationResult : ICertAuthenticationResult
{
    /// <summary>
    /// Gets the access token
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Gets the token expiration time
    /// </summary>
    public DateTimeOffset ExpiresOn { get; }

    /// <summary>
    /// Gets the refresh token if available
    /// </summary>
    public string? RefreshToken { get; }

    /// <summary>
    /// Gets the token type (e.g., "Bearer")
    /// </summary>
    public string TokenType { get; }

    /// <summary>
    /// Gets the scopes associated with the token
    /// </summary>
    public IReadOnlyCollection<string> Scopes { get; }

    /// <summary>
    /// Gets whether the authentication was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Creates a new successful CertAuthenticationResult
    /// </summary>
    /// <param name="accessToken">Access token</param>
    /// <param name="expiresOn">Expiration time</param>
    /// <param name="refreshToken">Refresh token (optional)</param>
    /// <param name="tokenType">Token type (defaults to "Bearer")</param>
    /// <param name="scopes">Token scopes (optional)</param>
    public CertAuthenticationResult(
        string accessToken,
        DateTimeOffset expiresOn,
        string? refreshToken = null,
        string tokenType = "Bearer",
        IEnumerable<string>? scopes = null)
    {
        AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        ExpiresOn = expiresOn;
        RefreshToken = refreshToken;
        TokenType = tokenType ?? "Bearer";
        Scopes = scopes != null ? new List<string>(scopes).AsReadOnly() : Array.Empty<string>();
        IsSuccess = true;
        ErrorMessage = null;
    }

    /// <summary>
    /// Creates a new failed CertAuthenticationResult
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    public CertAuthenticationResult(string errorMessage)
    {
        AccessToken = string.Empty;
        ExpiresOn = DateTimeOffset.MinValue;
        RefreshToken = null;
        TokenType = string.Empty;
        Scopes = Array.Empty<string>();
        IsSuccess = false;
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }

    /// <summary>
    /// Creates a successful authentication result
    /// </summary>
    public static CertAuthenticationResult Success(
        string accessToken,
        DateTimeOffset expiresOn,
        string? refreshToken = null,
        string tokenType = "Bearer",
        IEnumerable<string>? scopes = null)
    {
        return new CertAuthenticationResult(accessToken, expiresOn, refreshToken, tokenType, scopes);
    }

    /// <summary>
    /// Creates a failed authentication result
    /// </summary>
    public static CertAuthenticationResult Failure(string errorMessage)
    {
        return new CertAuthenticationResult(errorMessage);
    }
}
