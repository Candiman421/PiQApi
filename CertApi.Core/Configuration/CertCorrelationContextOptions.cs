// CertApi.Core/Configuration/CertCorrelationContextOptions.cs
namespace CertApi.Core.Configuration;

/// <summary>
/// Options for configuring correlation context behavior
/// </summary>
public record CertCorrelationContextOptions
{
    /// <summary>
    /// Gets or sets whether correlation IDs should be propagated in exceptions
    /// </summary>
    public bool PropagateCorrelationIdsInExceptions { get; init; } = true;

    /// <summary>
    /// Gets or sets whether correlation IDs should be included in logging
    /// </summary>
    public bool IncludeCorrelationIdsInLogging { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to create async local scopes
    /// </summary>
    public bool CreateAsyncLocalScopes { get; init; } = true;
}