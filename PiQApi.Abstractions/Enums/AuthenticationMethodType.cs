// PiQApi.Abstractions/Enums/AuthenticationMethodType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines authentication methods supported by the system
/// </summary>
public enum AuthenticationMethodType
{
    /// <summary>
    /// No authentication
    /// </summary>
    None = 0,

    /// <summary>
    /// OAuth authentication
    /// </summary>
    OAuth = 1,

    /// <summary>
    /// Client credentials flow authentication
    /// </summary>
    ClientCredentials = 2,

    /// <summary>
    /// Username/password authentication
    /// </summary>
    UsernamePassword = 3,

    /// <summary>
    /// Azure managed identity authentication
    /// </summary>
    ManagedIdentity = 4,

    /// <summary>
    /// Delegated authentication using tokens acquired on behalf of users
    /// </summary>
    DelegatedAuth = 5,
    
    /// <summary>
    /// Certificate-based authentication
    /// </summary>
    Certificate = 6,
    
    /// <summary>
    /// OpenID Connect authentication
    /// </summary>
    OIDC = 7
}