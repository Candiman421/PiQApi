// PiQApi.Abstractions/Configuration/IPiQEndpointConfiguration.cs
using PiQApi.Abstractions.Validation.Models;

namespace PiQApi.Abstractions.Configuration;

/// <summary>
/// Defines common configuration for service endpoints
/// </summary>
public interface IPiQEndpointConfiguration
{
    /// <summary>
    /// Gets the endpoint name
    /// </summary>
    string EndpointName { get; }

    /// <summary>
    /// Gets the endpoint URI
    /// </summary>
    Uri? EndpointUri { get; }

    /// <summary>
    /// Gets the maximum retries
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Gets the client ID
    /// </summary>
    string? ClientId { get; }

    /// <summary>
    /// Gets the tenant ID
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Configures the endpoint asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ConfigureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the endpoint configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<PiQValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
}