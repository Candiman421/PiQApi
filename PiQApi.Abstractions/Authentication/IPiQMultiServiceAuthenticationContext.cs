// PiQApi.Abstractions/Authentication/IPiQMultiServiceAuthenticationContext.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Manages authentication across multiple service endpoints
/// </summary>
public interface IPiQMultiServiceAuthenticationContext : IPiQCorrelationAware
{
    /// <summary>
    /// Gets all registered service endpoints
    /// </summary>
    IReadOnlyDictionary<ServiceEndpointType, IPiQAuthenticationContext> RegisteredServices { get; }

    /// <summary>
    /// Gets the authentication context for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <returns>Authentication context for the service</returns>
    IPiQAuthenticationContext GetAuthenticationContext(ServiceEndpointType serviceType);

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
    Task RegisterServiceAsync(ServiceEndpointType serviceType, IPiQAuthenticationOptions options, CancellationToken cancellationToken = default);

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