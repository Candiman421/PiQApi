// PiQApi.Core/Exceptions/Base/CertServiceException.cs
namespace PiQApi.Core.Exceptions.Base;

/// <summary>
/// Base class for service-related exceptions
/// </summary>
public abstract class CertServiceException : CertException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceException"/> class
    /// </summary>
    protected CertServiceException()
        : base("A service error occurred", "ServiceError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The error message</param>
    protected CertServiceException(string message)
        : base(message, "ServiceError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceException"/> class with a specified error message and error code
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    protected CertServiceException(string message, string? errorCode)
        : base(message, errorCode ?? "ServiceError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceException"/> class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="inner">The inner exception</param>
    protected CertServiceException(string message, Exception? inner)
        : base(message, "ServiceError", inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceException"/> class with a specified error message, error code, and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    /// <param name="inner">The inner exception</param>
    protected CertServiceException(string message, string? errorCode, Exception? inner)
        : base(message, errorCode ?? "ServiceError", inner)
    {
    }
}