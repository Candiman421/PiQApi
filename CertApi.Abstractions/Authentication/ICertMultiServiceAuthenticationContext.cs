// CertApi.Abstractions/Authentication/ICertMultiServiceAuthenticationContext.cs
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Authentication;

/// <summary>
/// Manages authentication across multiple service endpoints
/// </summary>
public interface ICertMultiServiceAuthenticationContext : ICertCorrelationAware
{
    /// <summary>
    /// Gets all registered service endpoints
    /// </summary>
    IReadOnlyDictionary<ServiceEndpointType, ICertAuthenticationContext> RegisteredServices { get; }

    /// <summary>
    /// Gets the authentication context for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <returns>Authentication context for the service</returns>
    ICertAuthenticationContext GetAuthenticationContext(ServiceEndpointType serviceType);

    /// <summary>
    /// Acquires a token for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token for the service</returns>
    Task<string> AcquireTokenAsync(ServiceEndpointType serviceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new service with authentication options
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="options">Authentication options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RegisterServiceAsync(ServiceEndpointType serviceType, ICertAuthenticationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the token for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the token is valid; otherwise, false</returns>
    Task<bool> ValidateTokenAsync(ServiceEndpointType serviceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the token for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateTokenAsync(ServiceEndpointType serviceType, CancellationToken cancellationToken = default);
}