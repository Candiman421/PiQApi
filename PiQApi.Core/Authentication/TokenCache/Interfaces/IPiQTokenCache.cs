// PiQApi.Core/Authentication/TokenCache/Interfaces/IPiQTokenCache.cs
namespace PiQApi.Core.Authentication.TokenCache.Interfaces;

/// <summary>
/// Interface for token caching
/// </summary>
public interface IPiQTokenCache
{
    /// <summary>
    /// Gets a token from the cache
    /// </summary>
    /// <param name="cacheKey">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token if found and not expired; otherwise null</returns>
    Task<PiQTokenInfo?> GetTokenAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a token in the cache
    /// </summary>
    /// <param name="cacheKey">Cache key</param>
    /// <param name="token">Token to cache</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task SetTokenAsync(string cacheKey, PiQTokenInfo token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a token from the cache
    /// </summary>
    /// <param name="cacheKey">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task RemoveTokenAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all tokens from the cache
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of tokens in the cache
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The count of tokens</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
