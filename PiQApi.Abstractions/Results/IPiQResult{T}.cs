// PiQApi.Abstractions/Results/IPiQResult{T}.cs
namespace PiQApi.Abstractions.Results;

/// <summary>
/// Defines a result type containing a value
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public interface IPiQResult<out T> : IPiQResult
{
    /// <summary>
    /// Gets the result value
    /// </summary>
    T? Value { get; }

    /// <summary>
    /// Creates a new result with additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New result with added property</returns>
    new IPiQResult<T> WithProperty(string key, object value);

    /// <summary>
    /// Creates a new result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New result with added properties</returns>
    new IPiQResult<T> WithProperties(IDictionary<string, object> properties);
}
