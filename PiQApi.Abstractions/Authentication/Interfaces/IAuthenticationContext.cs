// PiQApi.Abstractions/Authentication/Interfaces/IAuthenticationContext.cs
namespace PiQApi.Abstractions.Authentication.Interfaces
{
    /// <summary>
    /// Manages authentication state and token acquisition
    /// </summary>
    public interface IAuthenticationContext
    {
        /// <summary>
        /// Gets the current access token
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets whether the context is authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the token expiration time
        /// </summary>
        DateTime TokenExpiration { get; }

        /// <summary>
        /// Acquires a token asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Access token</returns>
        Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the current token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the token is valid</returns>
        Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates the current token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task InvalidateTokenAsync(CancellationToken cancellationToken = default);
    }
}