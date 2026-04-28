// CertApi.Abstractions/Authentication/ICertAuthenticationResult.cs
namespace CertApi.Abstractions.Authentication;

/// <summary>
/// Represents the result of an authentication operation
/// </summary>
public interface ICertAuthenticationResult
{
    /// <summary>
    /// Gets the access token
    /// </summary>
    string AccessToken { get; }

    /// <summary>
    /// Gets the token expiration time
    /// </summary>
    DateTimeOffset ExpiresOn { get; }
    
    /// <summary>
    /// Gets the refresh token if available
    /// </summary>
    string? RefreshToken { get; }
    
    /// <summary>
    /// Gets the token type (e.g., "Bearer")
    /// </summary>
    string TokenType { get; }
    
    /// <summary>
    /// Gets the scopes associated with the token
    /// </summary>
    IReadOnlyCollection<string> Scopes { get; }
    
    /// <summary>
    /// Gets whether the authentication was successful
    /// </summary>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Gets the error message if authentication failed
    /// </summary>
    string? ErrorMessage { get; }
}