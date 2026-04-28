// PiQApi.Core/Authentication/Credentials/PiQTokenRequestContext.cs
namespace PiQApi.Core.Authentication.Credentials;

/// <summary>
/// Context for token requests
/// </summary>
public class PiQTokenRequestContext
{
    private readonly string[] _scopes;

    /// <summary>
    /// Gets the scopes for the token request
    /// </summary>
    public IReadOnlyList<string> Scopes => _scopes;

    /// <summary>
    /// Creates a new PiQTokenRequestContext with the specified scopes
    /// </summary>
    /// <param name="scopes">Token scopes</param>
    public PiQTokenRequestContext(string[] scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        _scopes = scopes;
    }

    /// <summary>
    /// Creates a new PiQTokenRequestContext with the specified scopes
    /// </summary>
    /// <param name="scopes">Token scopes</param>
    public PiQTokenRequestContext(IEnumerable<string> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        _scopes = scopes.ToArray();
    }
}