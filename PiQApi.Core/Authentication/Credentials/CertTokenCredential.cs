// PiQApi.Core/Authentication/Credentials/CertTokenCredential.cs
using PiQApi.Abstractions.Authentication.Models;

namespace PiQApi.Core.Authentication.Credentials;

/// <summary>
/// Abstract base class for token credential implementations
/// </summary>
public abstract class CertTokenCredential
{
    /// <summary>
    /// Gets a token from this credential
    /// </summary>
    /// <param name="context">Token request context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication token with details</returns>
    public abstract Task<CertAccessToken> GetTokenAsync(CertTokenRequestContext context, CancellationToken cancellationToken = default);
}
