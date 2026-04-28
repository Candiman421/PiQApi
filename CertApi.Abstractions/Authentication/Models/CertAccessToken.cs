// CertApi.Abstractions/Authentication/Models/CertAccessToken.cs
namespace CertApi.Abstractions.Authentication.Models;

/// <summary>
/// Access token result
/// </summary>
public class CertAccessToken
{
    /// <summary>
    /// Gets the token string
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Gets the token expiration time
    /// </summary>
    public DateTimeOffset ExpiresOn { get; }

    /// <summary>
    /// Creates a new access token
    /// </summary>
    /// <param name="token">Token string</param>
    /// <param name="expiresOn">Expiration time</param>
    public CertAccessToken(string token, DateTimeOffset expiresOn)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpiresOn = expiresOn;
    }
}
