// CertApi.Abstractions/Core/ICertCorrelationId.cs
namespace CertApi.Abstractions.Core;

/// <summary>
/// Represents a correlation identifier for tracing operations across system boundaries
/// </summary>
public interface ICertCorrelationId
{
    /// <summary>
    /// Gets the unique identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the unique identifier value (alias for Id)
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Gets the collection of correlation properties
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    DateTime CreatedUtc { get; }

    /// <summary>
    /// Creates a new correlation identifier with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>A new correlation identifier with the additional property</returns>
    ICertCorrelationId WithProperty(string key, object value);

    /// <summary>
    /// Creates a new correlation identifier with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>A new correlation identifier with the additional properties</returns>
    ICertCorrelationId WithProperties(IDictionary<string, object> properties);

    /// <summary>
    /// Gets a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <returns>Property value or default</returns>
    T? GetProperty<T>(string key);

    /// <summary>
    /// Tries to get a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <param name="value">Output property value</param>
    /// <returns>True if property exists and is of the correct type</returns>
    bool TryGetProperty<T>(string key, out T? value);

    /// <summary>
    /// Checks if a property exists
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>True if the property exists</returns>
    bool HasProperty(string key);
}