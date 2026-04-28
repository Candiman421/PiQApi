// PiQApi.Core/Results/CertServiceResult{T}.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;

namespace PiQApi.Core.Results;

/// <summary>
/// Generic implementation of ICertServiceResult
/// </summary>
/// <typeparam name="T">Result value type</typeparam>
public class CertServiceResult<T> : CertServiceResult, ICertServiceResult<T>, ICertResult<T>
{
    /// <summary>
    /// Gets the result value
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Creates a new successful service result with a value
    /// </summary>
    /// <param name="value">Result value</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful service result with value</returns>
    protected internal static CertServiceResult<T> CreateSuccess(
        T value,
        OperationStatusType status,
        string requestId,
        string? correlationId = null)
    {
        // For success results with correlationId, create a success marker
        ICertResultError? successMarker = correlationId != null
            ? new CertSuccessMarker(correlationId)
            : null;

        return new CertServiceResult<T>(true, value, successMarker, status, requestId, correlationId ?? string.Empty, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates a new failure service result
    /// </summary>
    /// <param name="code">ErrorInfo code</param>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure service result</returns>
    protected internal static new CertServiceResult<T> CreateFailure(
        string code,
        string message,
        OperationStatusType status,
        string requestId,
        string? correlationId = null)
    {
        var error = new CertError(code, message, correlationId);
        return new CertServiceResult<T>(false, default, error, status, requestId, correlationId ?? error.CorrelationId, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceResult{T}"/> class
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful</param>
    /// <param name="value">Result value</param>
    /// <param name="error">ErrorInfo information</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timestamp">Operation timestamp</param>
    /// <param name="properties">Result properties</param>
    protected CertServiceResult(
        bool isSuccess,
        T? value,
        ICertResultError? error,
        OperationStatusType status,
        string requestId,
        string correlationId,
        DateTimeOffset timestamp,
        IDictionary<string, object>? properties = null)
        : base(isSuccess, error, status, requestId, correlationId, timestamp, properties)
    {
        Value = value;
    }

    /// <summary>
    /// Adds a property to the result - ICertServiceResult{T} implementation
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>A new result with the added property</returns>
    public new ICertServiceResult<T> WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        var newProperties = new Dictionary<string, object>(Properties);
        newProperties[key] = value;

        return new CertServiceResult<T>(IsSuccess, Value, ErrorInfo, Status, RequestId, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Adds multiple properties to the result - ICertServiceResult{T} implementation
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>A new result with the added properties</returns>
    public new ICertServiceResult<T> WithProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var newProperties = new Dictionary<string, object>(Properties);
        foreach (var kvp in properties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                newProperties[kvp.Key] = kvp.Value;
            }
        }

        return new CertServiceResult<T>(IsSuccess, Value, ErrorInfo, Status, RequestId, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Updates the result status - ICertServiceResult{T} implementation
    /// </summary>
    /// <param name="status">New status</param>
    /// <returns>A new service result with the updated status</returns>
    public new ICertServiceResult<T> WithStatus(OperationStatusType status)
    {
        return new CertServiceResult<T>(IsSuccess, Value, ErrorInfo, status, RequestId, CorrelationId, Timestamp,
            Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    /// <summary>
    /// ICertResult{T} implementation of WithProperty
    /// </summary>
    ICertResult<T> ICertResult<T>.WithProperty(string key, object value)
    {
        return WithProperty(key, value);
    }

    /// <summary>
    /// ICertResult{T} implementation of WithProperties
    /// </summary>
    ICertResult<T> ICertResult<T>.WithProperties(IDictionary<string, object> properties)
    {
        return WithProperties(properties);
    }
}
