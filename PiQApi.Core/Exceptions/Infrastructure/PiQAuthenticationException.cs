// PiQApi.Core/Exceptions/Infrastructure/PiQAuthenticationException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception that represents an authentication failure
/// </summary>
public class PiQAuthenticationException : PiQServiceException
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
    /// Initializes a new instance of the <see cref="PiQAuthenticationException"/> class
    /// </summary>
    public PiQAuthenticationException()
        : base("Authentication failed")
    {
        ServiceUri = null;
        TokenId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQAuthenticationException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PiQAuthenticationException(string message)
        : base(message)
    {
        ServiceUri = null;
        TokenId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQAuthenticationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQAuthenticationException(string message, Exception? inner)
        : base(message, inner)
    {
        ServiceUri = null;
        TokenId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQAuthenticationException"/> class with a specified error message,
    /// error code, service URI, token ID, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    /// <param name="serviceUri">The service URI associated with this exception</param>
    /// <param name="tokenId">The token ID associated with this exception</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQAuthenticationException(string message, string errorCode, Uri? serviceUri, string tokenId, Exception? inner = null)
        : base(message, errorCode, inner)
    {
        ServiceUri = serviceUri;
        TokenId = tokenId ?? string.Empty;

        AddData(nameof(ServiceUri), ServiceUri?.ToString() ?? "null");
        AddData(nameof(TokenId), TokenId);
    }
}