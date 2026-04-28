// PiQApi.Abstractions/Authentication/ICertTokenProvider.cs
namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Provides token acquisition capabilities
/// </summary>
public interface ICertTokenProvider
{
    /// <summary>
    /// Gets an access token string
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token string</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token
    /// </summary>
    /// <param name="accessToken">Current access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refreshed access token string</returns>
    Task<string> RefreshTokenAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a token using the specified options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication token info</returns>
    Task<ICertTokenInfo> GetTokenAsync(ICertAuthenticationOptions options, CancellationToken ct);

    /// <summary>
    /// Refreshes a token
    /// </summary>
    /// <param name="token">Token to refresh</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The refreshed token</returns>
    Task<ICertTokenInfo> RefreshTokenAsync(ICertTokenInfo token, CancellationToken ct = default);
}