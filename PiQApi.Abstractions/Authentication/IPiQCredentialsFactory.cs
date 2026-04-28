// PiQApi.Abstractions/Authentication/IPiQCredentialsFactory.cs
namespace PiQApi.Abstractions.Authentication;

/// <summary>
/// Factory for creating credential instances
/// </summary>
public interface IPiQCredentialsFactory
{
    /// <summary>
    /// Creates a credential from authentication options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <returns>A credential object</returns>
    object CreateCredential(IPiQAuthenticationOptions options);
}
