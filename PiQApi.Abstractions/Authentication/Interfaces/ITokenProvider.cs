// PiQApi.Abstractions/Authentication/Interfaces/ITokenProvider.cs
namespace PiQApi.Abstractions.Authentication.Interfaces
{
    /// <summary>
    /// Provides access tokens for authentication
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Gets an access token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Access token</returns>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes an existing token
        /// </summary>
        /// <param name="accessToken">Existing access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refreshed access token</returns>
        Task<string> RefreshTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}