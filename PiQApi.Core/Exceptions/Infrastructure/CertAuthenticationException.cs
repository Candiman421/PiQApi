// PiQApi.Core/Exceptions/Infrastructure/CertAuthenticationException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception that represents an authentication failure
/// </summary>
public class CertAuthenticationException : CertServiceException
{
    /// <summary>
    /// Gets the service URI associated with this exception
    /// </summary>
    public Uri? ServiceUri { get; }

    /// <summary>
    /// Gets the token ID associated with this exception
    /// </summary>
    public string TokenId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertAuthenticationException"/> class
    /// </summary>
    public CertAuthenticationException()
        : base("Authentication failed")
    {
        ServiceUri = null;
        TokenId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertAuthenticationException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public CertAuthenticationException(string message)
        : base(message)
    {
        ServiceUri = null;
        TokenId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertAuthenticationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertAuthenticationException(string message, Exception? inner)
        : base(message, inner)
    {
        ServiceUri = null;
        TokenId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertAuthenticationException"/> class with a specified error message,
    /// error code, service URI, token ID, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    /// <param name="serviceUri">The service URI associated with this exception</param>
    /// <param name="tokenId">The token ID associated with this exception</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertAuthenticationException(string message, string errorCode, Uri? serviceUri, string tokenId, Exception? inner = null)
        : base(message, errorCode, inner)
    {
        ServiceUri = serviceUri;
        TokenId = tokenId ?? string.Empty;

        AddData(nameof(ServiceUri), ServiceUri?.ToString() ?? "null");
        AddData(nameof(TokenId), TokenId);
    }
}