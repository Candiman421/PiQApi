// PiQApi.Core/Exceptions/Base/PiQServiceExceptionBase.cs
namespace PiQApi.Core.Exceptions.Base;

/// <summary>
/// Base exception class for service-related exceptions with service name and operation details
/// </summary>
public class PiQServiceExceptionBase : PiQServiceException
{
    /// <summary>
    /// Gets the service name related to this exception
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the operation name related to this exception
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceExceptionBase"/> class
    /// </summary>
    public PiQServiceExceptionBase()
        : base("A service error occurred", "ServiceError")
    {
        ServiceName = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceExceptionBase"/> class with a specified error message
    /// </summary>
    /// <param name="message">The error message</param>
    public PiQServiceExceptionBase(string message)
        : base(message, "ServiceError")
    {
        ServiceName = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceExceptionBase"/> class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="inner">The inner exception</param>
    public PiQServiceExceptionBase(string message, Exception? inner)
        : base(message, "ServiceError", inner)
    {
        ServiceName = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceExceptionBase"/> class with a specified error message and error code
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    public PiQServiceExceptionBase(string message, string? errorCode)
        : base(message, errorCode ?? "ServiceError")
    {
        ServiceName = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceExceptionBase"/> class with a specified error message, error code, and service name
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    /// <param name="serviceName">The service name</param>
    public PiQServiceExceptionBase(string message, string? errorCode, string? serviceName)
        : base(message, errorCode ?? "ServiceError")
    {
        ServiceName = serviceName ?? string.Empty;
        Operation = string.Empty;

        if (!string.IsNullOrEmpty(serviceName))
        {
            AddData(nameof(ServiceName), serviceName);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceExceptionBase"/> class with a specified error message, error code, service name, and operation
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    /// <param name="serviceName">The service name</param>
    /// <param name="operation">The operation name</param>
    public PiQServiceExceptionBase(string message, string? errorCode, string? serviceName, string? operation)
        : base(message, errorCode ?? "ServiceError")
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
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQServiceExceptionBase"/> class with a specified error message, error code, service name, operation, and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    /// <param name="serviceName">The service name</param>
    /// <param name="operation">The operation name</param>
    /// <param name="inner">The inner exception</param>
    public PiQServiceExceptionBase(string message, string? errorCode, string? serviceName, string? operation, Exception? inner)
        : base(message, errorCode ?? "ServiceError", inner)
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
    }
}