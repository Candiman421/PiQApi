// PiQApi.Abstractions/Authentication/ICertAuthenticationProvider.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Provides comprehensive authentication services
/// </summary>
public interface ICertAuthenticationProvider : ICertTokenProvider, ICertTokenValidator, ICertImpersonationProvider, ICertCorrelationAware
{
    /// <summary>
    /// Gets the authentication type
    /// </summary>
    AuthenticationMethodType AuthType { get; }
}