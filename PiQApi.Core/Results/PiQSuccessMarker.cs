// PiQApi.Core/Results/PiQSuccessMarker.cs
using PiQApi.Abstractions.Results;

namespace PiQApi.Core.Results;

/// <summary>
/// Success marker that implements IPiQResultError to carry correlation ID for successful results
/// </summary>
internal sealed class PiQSuccessMarker : IPiQResultError
{
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    /// <summary>
    /// Gets the error code (success for success markers)
    /// </summary>
    public string Code => "Success";

    /// <summary>
    /// Gets the error message (empty for success markers)
    /// </summary>
    public string Message => string.Empty;

    /// <summary>
    /// Gets the correlation ID associated with the operation
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when the operation occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the additional properties (empty for success markers)
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQSuccessMarker"/> class
    /// </summary>
    /// <param name="correlationId">The correlation ID to associate with the success result</param>
    public PiQSuccessMarker(string correlationId)
    {
        CorrelationId = correlationId ?? string.Empty;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQSuccessMarker"/> class with properties
    /// </summary>
    /// <param name="correlationId">The correlation ID</param>
    /// <param name="timestamp">The timestamp when the operation occurred</param>
    /// <param name="properties">Additional properties</param>
    private PiQSuccessMarker(string correlationId, DateTimeOffset timestamp, IDictionary<string, object>? properties)
    {
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
    /// Creates a new success marker with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New success marker with added property</returns>
    public IPiQResultError WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        var newProperties = new Dictionary<string, object>(Properties);
        newProperties[key] = value;

        return new PiQSuccessMarker(CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Creates a new success marker with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New success marker with added properties</returns>
    public IPiQResultError WithProperties(IDictionary<string, object> properties)
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

        return new PiQSuccessMarker(CorrelationId, Timestamp, newProperties);
    }

    /// <summary>
    /// Returns a string representation of the success marker
    /// </summary>
    /// <returns>A string containing the success marker correlation ID</returns>
    public override string ToString()
    {
        return $"Success (Correlation ID: {CorrelationId})";
    }
}