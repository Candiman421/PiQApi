// PiQApi.Core/Authentication/Credentials/PiQTokenCredential.cs
using PiQApi.Abstractions.Authentication.Models;

namespace PiQApi.Core.Authentication.Credentials;

/// <summary>
/// Abstract base class for token credential implementations
/// </summary>
public abstract class PiQTokenCredential
{
    /// <summary>
    /// Gets a token from this credential
    /// </summary>
    /// <param name="context">Token request context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication token with details</returns>
    public abstract Task<PiQAccessToken> GetTokenAsync(PiQTokenRequestContext context, CancellationToken cancellationToken = default);
}
