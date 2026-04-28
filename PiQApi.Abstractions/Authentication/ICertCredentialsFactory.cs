// PiQApi.Abstractions/Authentication/ICertCredentialsFactory.cs
namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Factory for creating credential instances
/// </summary>
public interface ICertCredentialsFactory
{
    /// <summary>
    /// Creates a credential from authentication options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <returns>A credential object</returns>
    object CreateCredential(ICertAuthenticationOptions options);
}
