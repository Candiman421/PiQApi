// PiQApi.Core/Exceptions/Base/PiQServiceException.cs
namespace PiQApi.Core.Exceptions.Base;

/// <summary>
/// Base class for service-related exceptions
/// </summary>
public abstract class PiQServiceException : PiQException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceException"/> class
    /// </summary>
    protected PiQServiceException()
        : base("A service error occurred", "ServiceError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The error message</param>
    protected PiQServiceException(string message)
        : base(message, "ServiceError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceException"/> class with a specified error message and error code
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    protected PiQServiceException(string message, string? errorCode)
        : base(message, errorCode ?? "ServiceError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceException"/> class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="inner">The inner exception</param>
    protected PiQServiceException(string message, Exception? inner)
        : base(message, "ServiceError", inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceException"/> class with a specified error message, error code, and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    /// <param name="inner">The inner exception</param>
    protected PiQServiceException(string message, string? errorCode, Exception? inner)
        : base(message, errorCode ?? "ServiceError", inner)
    {
    }
}