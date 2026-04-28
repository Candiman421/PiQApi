// PiQApi.Abstractions/Results/IPiQResult.cs
namespace PiQApi.Abstractions.Results;

/// <summary>
/// Defines a common result type for all operations
/// </summary>
public interface IPiQResult
{
    /// <summary>
    /// Gets whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the error information if the operation failed
    /// </summary>
    IPiQResultError? ErrorInfo { get; }

    /// <summary>
    /// Gets the correlation ID associated with this result
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when the result was created
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets additional result properties
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Creates a new result with additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New result with added property</returns>
    IPiQResult WithProperty(string key, object value);

    /// <summary>
    /// Creates a new result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New result with added properties</returns>
    IPiQResult WithProperties(IDictionary<string, object> properties);
}
