// CertApi.Abstractions/Enums/AuthenticationStatusType.cs
namespace CertApi.Abstractions.Enums;

/// <summary>
/// Represents the current status of authentication with the service
/// </summary>
public enum AuthenticationStatusType
{
    /// <summary>
    /// No authentication has been attempted
    /// </summary>
    Unauthenticated = 0,

    /// <summary>
    /// Successfully authenticated with valid credentials
    /// </summary>
    Authenticated = 1,

    /// <summary>
    /// Authentication token has expired
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Authentication credentials are invalid
    /// </summary>
    InvalidCredentials = 3,

    /// <summary>
    /// Authentication attempt failed
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Currently refreshing authentication token
    /// </summary>
    Refreshing = 5
}