// PiQApi.Core/Authentication/Credentials/CertTokenRequestContext.cs
namespace PiQApi.Core.Authentication.Credentials;

/// <summary>
/// Context for token requests
/// </summary>
public class CertTokenRequestContext
{
    private readonly string[] _scopes;

    /// <summary>
    /// Gets the scopes for the token request
    /// </summary>
    public IReadOnlyList<string> Scopes => _scopes;

    /// <summary>
    /// Creates a new CertTokenRequestContext with the specified scopes
    /// </summary>
    /// <param name="scopes">Token scopes</param>
    public CertTokenRequestContext(string[] scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        _scopes = scopes;
    }

    /// <summary>
    /// Creates a new CertTokenRequestContext with the specified scopes
    /// </summary>
    /// <param name="scopes">Token scopes</param>
    public CertTokenRequestContext(IEnumerable<string> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        _scopes = scopes.ToArray();
    }
}