// PiQApi.Core/Results/CertError.cs
using PiQApi.Abstractions.Results;

namespace PiQApi.Core.Results;

/// <summary>
/// Implementation of ICertResultError
/// </summary>
public class CertError : ICertResultError
{
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    /// <summary>
    /// Gets the error code
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the correlation ID associated with the error
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when the error occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the additional error properties
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertError"/> class
    /// </summary>
    /// <param name="code">ErrorInfo code</param>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="correlationId">Optional correlation ID</param>
    public CertError(string code, string message, string? correlationId = null)
    {
        Code = code ?? "CertError";
        Message = message ?? "An error occurred";
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertError"/> class with properties
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="message">Error message</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <param name="timestamp">Timestamp when the error occurred</param>
    /// <param name="properties">Additional error properties</param>
    protected CertError(string code, string message, string correlationId, DateTimeOffset timestamp, IDictionary<string, object>? properties)
    {
        Code = code;
        Message = message;
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
    /// Creates a new error with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New error with added property</returns>
    public ICertResultError WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        var newProperties = new Dictionary<string, object>(Properties);
        newProperties[key] = value;

        return new CertError(Code, Message, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Creates a new error with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New error with added properties</returns>
    public ICertResultError WithProperties(IDictionary<string, object> properties)
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

        return new CertError(Code, Message, CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Returns a string representation of the error
    /// </summary>
    /// <returns>A string containing the error code, message, and correlation ID</returns>
    public override string ToString()
    {
        return $"[{Code}] {Message} (Correlation ID: {CorrelationId})";
    }
}