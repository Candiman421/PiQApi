// PiQApi.Core/Authentication/CertAuthenticationOptionsValidator.cs
using Microsoft.Extensions.Options;

namespace PiQApi.Core.Authentication;

/// <summary>
/// Validator for authentication options
/// </summary>
public class CertAuthenticationOptionsValidator : IValidateOptions<CertAuthenticationOptions>
{
    /// <summary>
    /// Validates authentication options
    /// </summary>
    public ValidateOptionsResult Validate(string? name, CertAuthenticationOptions options)
    {
        // Skip validation for null options
        if (options == null)
            return ValidateOptionsResult.Skip;

        // Validate based on authentication type
        switch (options.AuthType)
        {
            case Abstractions.Enums.AuthenticationMethodType.ClientCredentials:
                if (string.IsNullOrEmpty(options.ClientId) ||
                    (string.IsNullOrEmpty(options.ClientSecret) && string.IsNullOrEmpty(options.CertificateThumbprint)))
                {
                    return ValidateOptionsResult.Fail(
                        "Client credentials authentication requires ClientId and either ClientSecret or CertificateThumbprint");
                }
                break;

            case Abstractions.Enums.AuthenticationMethodType.UsernamePassword:
                if (string.IsNullOrEmpty(options.Username) || string.IsNullOrEmpty(options.Password))
                {
                    return ValidateOptionsResult.Fail(
                        "Username/password authentication requires both Username and Password");
                }
                break;

            case Abstractions.Enums.AuthenticationMethodType.DelegatedAuth:
                if (string.IsNullOrEmpty(options.ClientId) || !options.Scopes.Any())
                {
                    return ValidateOptionsResult.Fail(
                        "Delegated authentication requires ClientId and at least one Scope");
                }
                break;
            case Abstractions.Enums.AuthenticationMethodType.None:
                break;
            case Abstractions.Enums.AuthenticationMethodType.OAuth:
                break;
            case Abstractions.Enums.AuthenticationMethodType.ManagedIdentity:
                break;
            default:
                break;
        }

        return ValidateOptionsResult.Success;
    }
}