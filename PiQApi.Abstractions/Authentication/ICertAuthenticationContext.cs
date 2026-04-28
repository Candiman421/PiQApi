// PiQApi.Abstractions/Authentication/ICertAuthenticationContext.cs
namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Defines the authentication context for maintaining token state
/// </summary>
public interface ICertAuthenticationContext
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
    /// Acquires an access token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token</returns>
    Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the current token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is valid; otherwise, false</returns>
    Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the current token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task InvalidateTokenAsync(CancellationToken cancellationToken = default);
}