// CertApi.Core/Results/CertResultError.cs
using CertApi.Abstractions.Results;

namespace CertApi.Core.Results;

/// <summary>
/// Implementation of error information for results
/// </summary>
public class CertResultError : ICertResultError
{
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation ID to track this error
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the error occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets additional error properties
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Creates a new empty error instance
    /// </summary>
    public CertResultError()
    {
    }

    /// <summary>
    /// Creates a new error with the specified code and message
    /// </summary>
    public CertResultError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Creates a new error with the specified details
    /// </summary>
    public CertResultError(string code, string message, string correlationId)
    {
        Code = code;
        Message = message;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Creates a new error with the specified details and properties
    /// </summary>
    public CertResultError(string code, string message, string correlationId, IDictionary<string, object>? properties = null)
    {
        Code = code;
        Message = message;
        CorrelationId = correlationId;

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
    /// Adds a property to the error
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    public void AddProperty(string key, object value)
    {
        if (!string.IsNullOrEmpty(key) && value != null)
        {
            _properties[key] = value;
        }
    }

    /// <summary>
    /// Adds multiple properties to the error
    /// </summary>
    /// <param name="properties">Properties to add</param>
    public void AddProperties(IDictionary<string, object> properties)
    {
        if (properties == null)
            return;

        foreach (var kvp in properties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                _properties[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Creates a new error with additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New error with added property</returns>
    public ICertResultError WithProperty(string key, object value)
    {
        var properties = new Dictionary<string, object>(Properties) { [key] = value };
        return new CertResultError(Code, Message, CorrelationId, properties);
    }

    /// <summary>
    /// Creates a new error with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New error with added properties</returns>
    public ICertResultError WithProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var mergedProperties = new Dictionary<string, object>(Properties);
        foreach (var property in properties)
        {
            mergedProperties[property.Key] = property.Value;
        }
        return new CertResultError(Code, Message, CorrelationId, mergedProperties);
    }
}