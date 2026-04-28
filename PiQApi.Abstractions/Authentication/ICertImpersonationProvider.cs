// PiQApi.Abstractions/Authentication/ICertImpersonationProvider.cs
namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Provides impersonation capabilities
/// </summary>
public interface ICertImpersonationProvider
{
    /// <summary>
    /// Determines whether impersonation is supported
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if impersonation is supported; otherwise, false</returns>
    Task<bool> SupportsImpersonationAsync(CancellationToken ct = default);

    /// <summary>
    /// Configures impersonation for the specified user
    /// </summary>
    /// <param name="userId">User ID to impersonate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ConfigureImpersonationAsync(string userId, CancellationToken ct);
}