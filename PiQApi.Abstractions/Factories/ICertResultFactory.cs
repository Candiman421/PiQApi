// PiQApi.Abstractions/Factories/ICertResultFactory.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;

namespace PiQApi.Abstractions.Factories;

/// <summary>
/// Factory for creating result objects
/// </summary>
public interface ICertResultFactory
{
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="value">Result value</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful result</returns>
    ICertResult<T> Success<T>(T value, string? correlationId = null);

    /// <summary>
    /// Creates a failure result from an exception
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="exception">Exception that caused the failure</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    ICertResult<T> Failure<T>(Exception exception, string? correlationId = null);

    /// <summary>
    /// Creates a failure result with code and message
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="code">ErrorInfo code</param>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    ICertResult<T> Failure<T>(string code, string message, string? correlationId = null);

    /// <summary>
    /// Creates a failure result with message and error code enum
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="errorCode">ErrorInfo code</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    ICertResult<T> Failure<T>(string message, ErrorCodeType errorCode, string? correlationId = null);

    /// <summary>
    /// Creates a successful service result
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="value">Result value</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful service result</returns>
    ICertServiceResult<T> ServiceSuccess<T>(T value, OperationStatusType status, string requestId, string? correlationId = null);

    /// <summary>
    /// Creates a failure service result
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="code">ErrorInfo code</param>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure service result</returns>
    ICertServiceResult<T> ServiceFailure<T>(string code, string message, OperationStatusType status, string requestId, string? correlationId = null);

    /// <summary>
    /// Creates a failure service result from an exception
    /// </summary>
    /// <typeparam name="T">Result value type</typeparam>
    /// <param name="exception">Exception that caused the failure</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure service result</returns>
    ICertServiceResult<T> ServiceFailure<T>(Exception exception, OperationStatusType status, string requestId, string? correlationId = null);
}
