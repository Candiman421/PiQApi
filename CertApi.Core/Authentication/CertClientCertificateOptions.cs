// CertApi.Core/Authentication/CertClientCertificateOptions.cs
using System.ComponentModel.DataAnnotations;

namespace CertApi.Core.Authentication;

/// <summary>
/// Options for client certificate credential
/// </summary>
public record CertClientCertificateOptions
{
    /// <summary>
    /// Gets or sets the authority host
    /// </summary>
    [Url]
    public Uri AuthorityHost { get; init; } = new Uri("https://login.microsoftonline.com");

    /// <summary>
    /// Gets or sets whether to validate the authority
    /// </summary>
    public bool ValidateAuthority { get; init; } = true;

    /// <summary>
    /// Gets or sets the certificate thumbprint
    /// </summary>
    [Required]
    public string? CertificateThumbprint { get; init; }
}