// PiQApi.Abstractions/Enums/AuthenticationStateType.cs

namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines authentication states for service operations.
/// </summary>
public enum AuthenticationStateType
{
    /// <summary>
    /// Authentication has not been initialized.
    /// </summary>
    NotInitialized = 0,

    /// <summary>
    /// Authentication is in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Authentication has been successfully completed.
    /// </summary>
    Authenticated = 2,

    /// <summary>
    /// Authentication has failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Authentication token has expired.
    /// </summary>
    TokenExpired = 4,

    /// <summary>
    /// Authentication service is unavailable.
    /// </summary>
    ServiceUnavailable = 5,
    
    /// <summary>
    /// Authentication token is being refreshed.
    /// </summary>
    Refreshing = 6,
    
    /// <summary>
    /// Authentication operation was canceled.
    /// </summary>
    Canceled = 7
}