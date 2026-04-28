// PiQApi.Abstractions/Authentication/Models/CertTokenRequestContext.cs
namespace PiQApi.Abstractions.Authentication.Models;

/// <summary>
/// Context for token requests
/// </summary>
public class CertTokenRequestContext
{
    /// <summary>
    /// Gets the scopes for the token request
    /// </summary>
    public IEnumerable<string> Scopes { get; }

    /// <summary>
    /// Gets or sets the tenant ID for the token request
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for the token request
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Creates a new token request context
    /// </summary>
    /// <param name="scopes">Token scopes</param>
    /// <param name="tenantId">Optional tenant ID</param>
    public CertTokenRequestContext(IEnumerable<string> scopes, string? tenantId = null)
    {
        Scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
        TenantId = tenantId;
    }
}