// CertApi.Abstractions/Authentication/ICertAuthenticationOptions.cs
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Authentication;

/// <summary>
/// Options for authentication
/// </summary>
public interface ICertAuthenticationOptions
{
    /// <summary>
    /// Gets the authentication type
    /// </summary>
    AuthenticationMethodType AuthType { get; }

    /// <summary>
    /// Gets the client ID
    /// </summary>
    string? ClientId { get; }

    /// <summary>
    /// Gets the tenant ID
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the resource identifier
    /// </summary>
    string? Resource { get; }

    /// <summary>
    /// Gets the authority URL
    /// </summary>
    string? Authority { get; }
}