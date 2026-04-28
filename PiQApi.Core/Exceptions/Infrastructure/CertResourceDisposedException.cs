// PiQApi.Core/Exceptions/Infrastructure/CertResourceDisposedException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when attempting to use a resource that has been disposed
/// </summary>
public class CertResourceDisposedException : CertServiceException
{
    /// <summary>
    /// Gets the name of the disposed object
    /// </summary>
    public string ObjectName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceDisposedException"/> class
    /// </summary>
    public CertResourceDisposedException()
        : base("Object has been disposed", "ObjectDisposed")
    {
        ObjectName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceDisposedException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public CertResourceDisposedException(string message)
        : base(message, "ObjectDisposed")
    {
        ObjectName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceDisposedException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertResourceDisposedException(string message, Exception? inner)
        : base(message, "ObjectDisposed", inner)
    {
        ObjectName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceDisposedException"/> class with a specified object name
    /// and error message
    /// </summary>
    /// <param name="objectName">The name of the disposed object</param>
    /// <param name="message">The message that describes the error</param>
    public CertResourceDisposedException(string objectName, string message)
        : base(message, "ObjectDisposed")
    {
        ObjectName = objectName ?? string.Empty;
        AddData(nameof(ObjectName), ObjectName);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceDisposedException"/> class with a specified object name,
    /// error message, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="objectName">The name of the disposed object</param>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertResourceDisposedException(string objectName, string message, Exception? inner)
        : base(message, "ObjectDisposed", inner)
    {
        ObjectName = objectName ?? string.Empty;
        AddData(nameof(ObjectName), ObjectName);
    }
}