// PiQApi.Abstractions/Authentication/ICertAuthenticationProviderFactory.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Authentication;

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