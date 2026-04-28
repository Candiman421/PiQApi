// PiQApi.Core/Exceptions/Base/PiQException.cs
using PiQApi.Abstractions.Exceptions;

namespace PiQApi.Core.Exceptions.Base;

/// <summary>
/// Base class for all certificate-related exceptions
/// </summary>
public abstract class PiQException : Exception, IPiQExceptionInfo
{
    private readonly Dictionary<string, object> _additionalData = new();

    /// <summary>
    /// Gets the error code for this exception
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the correlation identifier for tracing
    /// </summary>
    public string CorrelationId { get; private set; }

    /// <summary>
    /// Gets additional contextual data
    /// </summary>
    public IReadOnlyDictionary<string, object> AdditionalData => _additionalData;

    /// <summary>
    /// Gets the timestamp when the exception occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQException"/> class
    /// </summary>
    protected PiQException()
        : base("An error occurred")
    {
        ErrorCode = "PiQError";
        CorrelationId = Guid.NewGuid().ToString();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The error message</param>
    protected PiQException(string message)
        : base(message ?? "An error occurred")
    {
        ErrorCode = "PiQError";
        CorrelationId = Guid.NewGuid().ToString();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQException"/> class with a specified error message and error code
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    protected PiQException(string message, string errorCode)
        : base(message ?? "An error occurred")
    {
        ErrorCode = errorCode ?? "PiQError";
        CorrelationId = Guid.NewGuid().ToString();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQException"/> class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="inner">The inner exception</param>
    protected PiQException(string message, Exception? inner)
        : base(message ?? "An error occurred", inner)
    {
        ErrorCode = "PiQError";
        CorrelationId = inner is IPiQExceptionInfo certEx ? certEx.CorrelationId : Guid.NewGuid().ToString();
        Timestamp = DateTimeOffset.UtcNow;

        // Copy additional data from inner exception if it's a PiQException
        if (inner is IPiQExceptionInfo innerCertEx)
        {
            foreach (var kvp in innerCertEx.AdditionalData)
            {
                _additionalData[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQException"/> class with a specified error message, error code, and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    /// <param name="inner">The inner exception</param>
    protected PiQException(string message, string errorCode, Exception? inner)
        : base(message ?? "An error occurred", inner)
    {
        ErrorCode = errorCode ?? "PiQError";
        CorrelationId = inner is IPiQExceptionInfo certEx ? certEx.CorrelationId : Guid.NewGuid().ToString();
        Timestamp = DateTimeOffset.UtcNow;

        // Copy additional data from inner exception if it's a PiQException
        if (inner is IPiQExceptionInfo innerCertEx)
        {
            foreach (var kvp in innerCertEx.AdditionalData)
            {
                _additionalData[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Sets the correlation ID for this exception
    /// </summary>
    /// <param name="correlationId">The correlation ID to set</param>
    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId ?? string.Empty;
    }

    /// <summary>
    /// Adds additional data to the exception
    /// </summary>
    /// <param name="key">The data key</param>
    /// <param name="value">The data value</param>
    protected void AddData(string key, object? value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            _additionalData[key] = value ?? "null";
        }
    }
}