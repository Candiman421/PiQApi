// PiQApi.Core/Exceptions/Infrastructure/PiQGenericServiceException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Represents a generic service exception with service name and operation information
/// </summary>
public class PiQGenericServiceException : PiQServiceException
{
    /// <summary>
    /// Gets the name of the service where the exception occurred
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the name of the operation that caused the exception
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQGenericServiceException"/> class
    /// </summary>
    public PiQGenericServiceException()
        : base("A service error occurred", "ServiceError")
    {
        ServiceName = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQGenericServiceException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PiQGenericServiceException(string message)
        : base(message, "ServiceError")
    {
        ServiceName = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQGenericServiceException"/> class with a specified error message and error code
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    public PiQGenericServiceException(string message, string errorCode)
        : base(message, errorCode)
    {
        ServiceName = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQGenericServiceException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQGenericServiceException(string message, Exception? inner)
        : base(message, "ServiceError", inner)
    {
        ServiceName = string.Empty;
        Operation = string.Empty;

        // Preserve correlation ID from inner exception if it's a PiQException
        if (inner is PiQException certException)
        {
            SetCorrelationId(certException.CorrelationId);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQGenericServiceException"/> class with a specified error message,
    /// error code, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQGenericServiceException(string message, string errorCode, Exception? inner)
        : base(message, errorCode, inner)
    {
        ServiceName = string.Empty;
        Operation = string.Empty;

        // Preserve correlation ID from inner exception if it's a PiQException
        if (inner is PiQException certException)
        {
            SetCorrelationId(certException.CorrelationId);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQGenericServiceException"/> class with a specified error message,
    /// error code, service name, operation, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    /// <param name="serviceName">The name of the service where the exception occurred</param>
    /// <param name="operation">The name of the operation that caused the exception</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQGenericServiceException(string message, string errorCode, string? serviceName, string? operation, Exception? inner)
        : base(message, errorCode, inner)
    {
        ServiceName = serviceName ?? string.Empty;
        Operation = operation ?? string.Empty;

        if (!string.IsNullOrEmpty(serviceName))
        {
            AddData(nameof(ServiceName), serviceName);
        }

        if (!string.IsNullOrEmpty(operation))
        {
            AddData(nameof(Operation), operation);
        }

        // Preserve correlation ID from inner exception if it's a PiQException
        if (inner is PiQException certException)
        {
            SetCorrelationId(certException.CorrelationId);
        }
    }
}