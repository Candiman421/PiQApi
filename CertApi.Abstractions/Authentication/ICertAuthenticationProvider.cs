// CertApi.Abstractions/Authentication/ICertAuthenticationProvider.cs
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Authentication;

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