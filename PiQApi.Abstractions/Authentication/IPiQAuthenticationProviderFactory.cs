// PiQApi.Abstractions/Authentication/IPiQAuthenticationProviderFactory.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Factory for creating authentication providers based on authentication type
/// </summary>
public interface IPiQAuthenticationProviderFactory : IPiQCorrelationAware
{
    /// <summary>
    /// Creates an authentication provider for the specified authentication type
    /// </summary>
    /// <param name="authType">Authentication type</param>
    /// <returns>Authentication provider</returns>
    IPiQAuthenticationProvider CreateProvider(AuthenticationMethodType authType);

    /// <summary>
    /// Creates an authentication provider using the authentication type from options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <returns>Authentication provider</returns>
    IPiQAuthenticationProvider CreateProvider(IPiQAuthenticationOptions options);
}