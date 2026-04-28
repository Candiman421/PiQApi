// PiQApi.Core/Factories/CertResultFactory.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Exceptions;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Results;
using PiQApi.Core.Results;

namespace PiQApi.Core.Factories;

/// <summary>
/// Factory implementation for creating result objects
/// </summary>
public class CertResultFactory : ICertResultFactory
{
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="value">Result value</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful result</returns>
    public ICertResult<T> Success<T>(T value, string? correlationId = null)
    {
        return CertResult<T>.CreateSuccess(value, correlationId);
    }

    /// <summary>
    /// Creates a failure result from an exception
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="exception">Exception that caused the failure</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    public ICertResult<T> Failure<T>(Exception exception, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Extract correlation ID from exception if it's a CertException
        string? extractedCorrelationId = null;
        string errorCode = "CertError";

        if (exception is ICertExceptionInfo certException)
        {
            extractedCorrelationId = certException.CorrelationId;
            errorCode = certException.ErrorCode;
        }

        return CertResult<T>.CreateFailure(errorCode, exception.Message, correlationId ?? extractedCorrelationId);
    }

    /// <summary>
    /// Creates a failure result with code and message
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    public ICertResult<T> Failure<T>(string code, string message, string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return CertResult<T>.CreateFailure(code, message, correlationId);
    }

    /// <summary>
    /// Creates a failure result with message and error code enum
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    public ICertResult<T> Failure<T>(string message, ErrorCodeType errorCode, string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);

        return CertResult<T>.CreateFailure(errorCode.ToString(), message, correlationId);
    }

    /// <summary>
    /// Creates a successful service result
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="value">Result value</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful service result</returns>
    public ICertServiceResult<T> ServiceSuccess<T>(T value, OperationStatusType status, string requestId, string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(requestId);

        return CertServiceResult<T>.CreateSuccess(
            value,
            status,
            requestId,
            correlationId);
    }

    /// <summary>
    /// Creates a failure service result
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure service result</returns>
    public ICertServiceResult<T> ServiceFailure<T>(string code, string message, OperationStatusType status, string requestId, string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(requestId);

        return CertServiceResult<T>.CreateFailure(
            code,
            message,
            status,
            requestId,
            correlationId);
    }

    /// <summary>
    /// Creates a failure service result from an exception
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="exception">Exception that caused the failure</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure service result</returns>
    public ICertServiceResult<T> ServiceFailure<T>(Exception exception, OperationStatusType status, string requestId, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(requestId);

        // Extract correlation ID from exception if it's a CertException
        string? extractedCorrelationId = null;
        string errorCode = "CertError";

        if (exception is ICertExceptionInfo certException)
        {
            extractedCorrelationId = certException.CorrelationId;
            errorCode = certException.ErrorCode;
        }

        return CertServiceResult<T>.CreateFailure(
            errorCode,
            exception.Message,
            status,
            requestId,
            correlationId ?? extractedCorrelationId);
    }
}