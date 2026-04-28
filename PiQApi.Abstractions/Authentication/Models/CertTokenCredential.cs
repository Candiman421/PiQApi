// PiQApi.Abstractions/Authentication/Models/CertTokenCredential.cs
namespace PiQApi.Abstractions.Authentication.Models;

/// <summary>
/// Base class for token credentials
/// </summary>
public abstract class CertTokenCredential
{
    /// <summary>
    /// Gets an access token
    /// </summary>
    /// <param name="requestContext">Token request context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An access token</returns>
    public abstract Task<CertAccessToken> GetTokenAsync(
        CertTokenRequestContext requestContext,
        CancellationToken cancellationToken = default);
}