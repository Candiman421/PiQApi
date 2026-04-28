// PiQApi.Core/Results/CertResult.cs
using PiQApi.Abstractions.Results;

namespace PiQApi.Core.Results;

/// <summary>
/// Base implementation of ICertResult for non-generic scenarios
/// </summary>
public class CertResult : ICertResult
{
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error information if the operation failed
    /// </summary>
    public ICertResultError? ErrorInfo { get; }

    /// <summary>
    /// Gets the correlation ID associated with this result
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when the result was created
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets additional result properties
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Creates a new successful result
    /// </summary>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A successful result</returns>
    protected internal static CertResult CreateSuccess(string? correlationId = null)
    {
        ICertResultError? successMarker = correlationId != null
            ? new CertSuccessMarker(correlationId)
            : null;

        return new CertResult(true, successMarker, correlationId ?? string.Empty, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates a new failure result
    /// </summary>
    /// <param name="code">ErrorInfo code</param>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="correlationId">Optional correlation ID</param>
    /// <returns>A failure result</returns>
    protected internal static CertResult CreateFailure(string code, string message, string? correlationId = null)
    {
        var error = new CertError(code, message, correlationId);
        return new CertResult(false, error, correlationId ?? error.CorrelationId, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates a success marker with the specified correlation ID
    /// </summary>
    /// <param name="correlationId">Correlation ID to associate with the success marker</param>
    /// <returns>A success marker that implements ICertResultError</returns>
    public static ICertResultError CreateSuccessMarker(string correlationId)
    {
        return new CertSuccessMarker(correlationId);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResult"/> class
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful</param>
    /// <param name="error">ErrorInfo information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timestamp">Operation timestamp</param>
    /// <param name="properties">Result properties</param>
    protected CertResult(
        bool isSuccess,
        ICertResultError? error,
        string correlationId,
        DateTimeOffset timestamp,
        IDictionary<string, object>? properties = null)
    {
        IsSuccess = isSuccess;
        ErrorInfo = error;
        CorrelationId = correlationId;
        Timestamp = timestamp;

        if (properties != null)
        {
            foreach (var kvp in properties)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
                {
                    _properties[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    /// <summary>
    /// Creates a new result with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New result with added property</returns>
    public ICertResult WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        var newProperties = new Dictionary<string, object>(_properties);
        newProperties[key] = value;

        return new CertResult(IsSuccess, ErrorInfo, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Creates a new result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New result with added properties</returns>
    public ICertResult WithProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var newProperties = new Dictionary<string, object>(_properties);

        foreach (var kvp in properties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                newProperties[kvp.Key] = kvp.Value;
            }
        }

        return new CertResult(IsSuccess, ErrorInfo, CorrelationId, Timestamp, newProperties);
    }
}
