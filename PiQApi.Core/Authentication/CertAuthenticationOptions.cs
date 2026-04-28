// PiQApi.Core/Authentication/CertAuthenticationOptions.cs
using System.ComponentModel.DataAnnotations;
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Enums;
using PiQApi.Core.Resilience;

namespace PiQApi.Core.Authentication;

/// <summary>
/// Configuration options for authentication
/// </summary>
public record CertAuthenticationOptions : ICertAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the authentication method type
    /// </summary>
    public AuthenticationMethodType AuthType { get; init; } = AuthenticationMethodType.None;

    /// <summary>
    /// Gets or sets the tenant ID
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets or sets the client ID
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// Gets or sets the client secret
    /// </summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Gets or sets the certificate thumbprint
    /// </summary>
    public string? CertificateThumbprint { get; init; }

    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Gets or sets the resource
    /// </summary>
    public string? Resource { get; init; } = "https://outlook.office365.com";

    /// <summary>
    /// Gets the scopes
    /// </summary>
    public IEnumerable<string> Scopes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the token lifetime in minutes
    /// </summary>
    [Range(1, 1440)]
    public int TokenLifetimeMinutes { get; init; } = 60;

    /// <summary>
    /// Gets or sets the instance URL
    /// </summary>
    [Url]
    public string Instance { get; init; } = "https://login.microsoftonline.com/";

    /// <summary>
    /// Gets or sets the authority
    /// </summary>
    public string? Authority { get; init; }

    /// <summary>
    /// Gets or sets the cache key
    /// </summary>
    public string? CacheKey { get; init; }

    /// <summary>
    /// Gets or sets whether to validate the authority
    /// </summary>
    public bool ValidateAuthority { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to simulate the request
    /// </summary>
    public bool SimulateRequest { get; init; }

    /// <summary>
    /// Gets the retry options
    /// </summary>
    public CertRetryOptions RetryOptions { get; init; } = new CertRetryOptions();

    /// <summary>
    /// Gets or sets the client certificate options
    /// </summary>
    public ClientCertificateOptionsType? ClientCertificateOptions { get; init; }
}
