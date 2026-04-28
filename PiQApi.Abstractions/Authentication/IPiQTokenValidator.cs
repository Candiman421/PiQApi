// PiQApi.Abstractions/Authentication/IPiQTokenValidator.cs
namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Provides token validation capabilities
/// </summary>
public interface IPiQTokenValidator
{
    /// <summary>
    /// Validates a token
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if the token is valid; otherwise, false</returns>
    Task<bool> ValidateTokenAsync(IPiQTokenInfo token, CancellationToken ct);

    /// <summary>
    /// Invalidates a token
    /// </summary>
    /// <param name="token">Token to invalidate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task InvalidateTokenAsync(IPiQTokenInfo token, CancellationToken ct);
}