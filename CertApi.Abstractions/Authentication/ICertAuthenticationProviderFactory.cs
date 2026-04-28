// CertApi.Abstractions/Authentication/ICertAuthenticationProviderFactory.cs
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Authentication;

/// <summary>
/// Factory for creating authentication providers based on authentication type
/// </summary>
public interface ICertAuthenticationProviderFactory : ICertCorrelationAware
{
    /// <summary>
    /// Creates an authentication provider for the specified authentication type
    /// </summary>
    /// <param name="authType">Authentication type</param>
    /// <returns>Authentication provider</returns>
    ICertAuthenticationProvider CreateProvider(AuthenticationMethodType authType);

    /// <summary>
    /// Creates an authentication provider using the authentication type from options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <returns>Authentication provider</returns>
    ICertAuthenticationProvider CreateProvider(ICertAuthenticationOptions options);
}