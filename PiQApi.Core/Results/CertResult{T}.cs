// PiQApi.Core/Results/CertResult{T}.cs
using PiQApi.Abstractions.Results;

namespace PiQApi.Core.Results;

/// <summary>
/// Generic implementation of ICertResult
/// </summary>
/// <typeparam name="T">Result value type</typeparam>
public class CertResult<T> : CertResult, ICertResult<T>
{
    /// <summary>
    /// Gets the result value
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Creates a new successful result with a value
    /// </summary>
    /// <param name="value">Result value</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful result with value</returns>
    protected internal static CertResult<T> CreateSuccess(T value, string? correlationId = null)
    {
        // For success results, create a success marker with the correlationId if provided
        ICertResultError? successMarker = correlationId != null
            ? new CertSuccessMarker(correlationId)
            : null;

        return new CertResult<T>(true, value, successMarker, correlationId ?? string.Empty, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates a new failure result
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    protected internal static new CertResult<T> CreateFailure(string code, string message, string? correlationId = null)
    {
        var error = new CertError(code, message, correlationId);
        return new CertResult<T>(false, default, error, correlationId ?? error.CorrelationId, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResult{T}"/> class
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful</param>
    /// <param name="value">Result value</param>
    /// <param name="error">Error information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timestamp">Operation timestamp</param>
    /// <param name="properties">Result properties</param>
    protected CertResult(
        bool isSuccess,
        T? value,
        ICertResultError? error,
        string correlationId,
        DateTimeOffset timestamp,
        IDictionary<string, object>? properties = null)
        : base(isSuccess, error, correlationId, timestamp, properties)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new result with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New result with added property</returns>
    public new ICertResult<T> WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        var newProperties = new Dictionary<string, object>(Properties);
        newProperties[key] = value;

        return new CertResult<T>(IsSuccess, Value, ErrorInfo, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Creates a new result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New result with added properties</returns>
    public new ICertResult<T> WithProperties(IDictionary<string, object> properties)
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

        return new CertResult<T>(IsSuccess, Value, ErrorInfo, CorrelationId, Timestamp, newProperties);
    }
}