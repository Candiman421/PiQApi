// PiQApi.Core/Exceptions/Infrastructure/PiQConnectionException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception that represents a connection failure
/// </summary>
public class PiQConnectionException : PiQServiceException
{
    /// <summary>
    /// Gets the service URI associated with this exception
    /// </summary>
    public Uri? ServiceUri { get; }

    /// <summary>
    /// Gets the request ID associated with this exception
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConnectionException"/> class
    /// </summary>
    public PiQConnectionException()
        : base("Connection failed")
    {
        ServiceUri = null;
        RequestId = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConnectionException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PiQConnectionException(string message)
        : base(message)
    {
        ServiceUri = null;
        RequestId = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConnectionException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQConnectionException(string message, Exception? inner)
        : base(message, inner)
    {
        ServiceUri = null;
        RequestId = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConnectionException"/> class with a specified error message,
    /// service URI, request ID, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="serviceUri">The service URI associated with this exception</param>
    /// <param name="requestId">The request ID associated with this exception</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQConnectionException(string message, Uri? serviceUri, string? requestId, Exception? inner = null)
        : base(message, "ConnectionError", inner)
    {
        ServiceUri = serviceUri;
        RequestId = requestId;

        AddData(nameof(ServiceUri), ServiceUri?.ToString() ?? "null");

        if (!string.IsNullOrEmpty(RequestId))
        {
            AddData(nameof(RequestId), RequestId);
        }
    }
}