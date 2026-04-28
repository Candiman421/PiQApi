// PiQApi.Abstractions/Results/ICertResultError.cs
namespace PiQApi.Abstractions.Results;

/// <summary>
/// Defines error information for results
/// </summary>
public interface ICertResultError
{
    /// <summary>
    /// Gets the error code
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the error message
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the correlation ID to track this error
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when the error occurred
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets additional error properties
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Creates a new error with additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New error with added property</returns>
    ICertResultError WithProperty(string key, object value);

    /// <summary>
    /// Creates a new error with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New error with added properties</returns>
    ICertResultError WithProperties(IDictionary<string, object> properties);
}