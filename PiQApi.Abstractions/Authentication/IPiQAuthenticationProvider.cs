// PiQApi.Abstractions/Authentication/IPiQAuthenticationProvider.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Provides comprehensive authentication services
/// </summary>
public interface IPiQAuthenticationProvider : IPiQTokenProvider, IPiQTokenValidator, IPiQImpersonationProvider, IPiQCorrelationAware
{
    /// <summary>
    /// Gets the authentication type
    /// </summary>
    AuthenticationMethodType AuthType { get; }
}