// PiQApi.Abstractions/Context/IPiQOperationProperties.cs
namespace PiQApi.Abstractions.Context;

/// <summary>
/// Defines property management operations for an operation context
/// </summary>
public interface IPiQOperationProperties
{
    /// <summary>
    /// Gets a collection of properties specific to this operation
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Adds a property to the operation
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    void AddProperty(string key, object value);

    /// <summary>
    /// Adds multiple properties to the operation
    /// </summary>
    /// <param name="properties">Properties to add</param>
    void AddProperties(IDictionary<string, object> properties);

    /// <summary>
    /// Gets a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <returns>Property value</returns>
    T GetPropertyValue<T>(string key);

    /// <summary>
    /// Tries to get a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <param name="value">Output property value</param>
    /// <returns>True if property exists and is of correct type</returns>
    bool TryGetPropertyValue<T>(string key, out T? value);

    /// <summary>
    /// Checks if a property exists
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>True if property exists</returns>
    bool HasProperty(string key);
}