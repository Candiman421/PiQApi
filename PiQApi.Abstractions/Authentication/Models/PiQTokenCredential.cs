// PiQApi.Abstractions/Authentication/Models/PiQTokenCredential.cs
namespace PiQApi.Abstractions.Authentication.Models;

/// <summary>
/// Base class for token credentials
/// </summary>
public abstract class PiQTokenCredential
{
    /// <summary>
    /// Gets an access token
    /// </summary>
    /// <param name="requestContext">Token request context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An access token</returns>
    public abstract Task<PiQAccessToken> GetTokenAsync(
        PiQTokenRequestContext requestContext,
        CancellationToken cancellationToken = default);
}