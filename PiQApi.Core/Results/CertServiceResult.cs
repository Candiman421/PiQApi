// PiQApi.Core/Results/CertServiceResult.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;

namespace PiQApi.Core.Results;

/// <summary>
/// Base implementation of ICertServiceResult
/// </summary>
public class CertServiceResult : CertResult, ICertServiceResult
{
    /// <summary>
    /// Gets the operation status
    /// </summary>
    public OperationStatusType Status { get; }

    /// <summary>
    /// Gets the request ID
    /// </summary>
    public string RequestId { get; }

    /// <summary>
    /// Creates a new successful service result
    /// </summary>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful service result</returns>
    protected internal static CertServiceResult CreateSuccess(
        OperationStatusType status,
        string requestId,
        string? correlationId = null)
    {
        // For success results, create a success marker with the correlationId if provided
        ICertResultError? successMarker = correlationId != null
            ? new CertSuccessMarker(correlationId)
            : null;

        return new CertServiceResult(true, successMarker, status, requestId, correlationId ?? string.Empty, DateTimeOffset.UtcNow);
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
    protected internal static CertServiceResult CreateFailure(
        string code,
        string message,
        OperationStatusType status,
        string requestId,
        string? correlationId = null)
    {
        var error = new CertError(code, message, correlationId);
        return new CertServiceResult(false, error, status, requestId, correlationId ?? error.CorrelationId, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertServiceResult"/> class
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful</param>
    /// <param name="error">ErrorInfo information</param>
    /// <param name="status">Operation status</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timestamp">Operation timestamp</param>
    /// <param name="properties">Result properties</param>
    protected CertServiceResult(
        bool isSuccess,
        ICertResultError? error,
        OperationStatusType status,
        string requestId,
        string correlationId,
        DateTimeOffset timestamp,
        IDictionary<string, object>? properties = null)
        : base(isSuccess, error, correlationId, timestamp, properties)
    {
        Status = status;
        RequestId = requestId ?? string.Empty;
    }

    /// <summary>
    /// Creates a new service result with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New service result with added property</returns>
    public new ICertServiceResult WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        var newProperties = new Dictionary<string, object>(Properties);
        newProperties[key] = value;

        return new CertServiceResult(IsSuccess, ErrorInfo, Status, RequestId, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Creates a new service result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New service result with added properties</returns>
    public new ICertServiceResult WithProperties(IDictionary<string, object> properties)
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

        return new CertServiceResult(IsSuccess, ErrorInfo, Status, RequestId, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Creates a new service result with a different status
    /// </summary>
    /// <param name="status">New status</param>
    /// <returns>New service result with updated status</returns>
    public ICertServiceResult WithStatus(OperationStatusType status)
    {
        return new CertServiceResult(IsSuccess, ErrorInfo, status, RequestId, CorrelationId, Timestamp, new Dictionary<string, object>(Properties));
    }
}
