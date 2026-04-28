// PiQApi.Core/Core/Models/CertOperationId.cs
using System.Collections.Immutable;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Utilities.Time;

namespace PiQApi.Core.Core.Models;

/// <summary>
/// Represents a unique identifier for operations with additional metadata
/// </summary>
public sealed class CertOperationId : IEquatable<CertOperationId>
{
    private readonly ImmutableDictionary<string, object> _properties;

    // Replaced direct implementation dependency with factory access
    private static ICertTimeProvider DefaultTimeProvider => CertTimeProviderFactory.Current;

    /// <summary>
    /// Gets the unique identifier
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the operation name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTime CreatedUtc { get; }

    /// <summary>
    /// Gets the collection of operation properties
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Creates a new operation identifier
    /// </summary>
    /// <param name="id">The operation identifier</param>
    /// <param name="name">The operation name</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public CertOperationId(string id, string name, ICertTimeProvider? timeProvider = null)
        : this(id, name, ImmutableDictionary<string, object>.Empty, (timeProvider ?? DefaultTimeProvider).UtcNow)
    {
    }

    /// <summary>
    /// Creates a new operation identifier with specified properties and creation time
    /// </summary>
    /// <param name="id">The operation identifier</param>
    /// <param name="name">The operation name</param>
    /// <param name="properties">Properties dictionary</param>
    /// <param name="createdUtc">Creation timestamp</param>
    public CertOperationId(string id, string name, ImmutableDictionary<string, object> properties, DateTime createdUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(name);
        Id = id;
        Name = name;
        _properties = properties ?? ImmutableDictionary<string, object>.Empty;
        CreatedUtc = createdUtc;
    }

    /// <summary>
    /// Creates a new operation identifier with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>A new operation identifier with the additional property</returns>
    public CertOperationId WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
        return new CertOperationId(Id, Name, _properties.SetItem(key, value), CreatedUtc);
    }

    /// <summary>
    /// Creates a new operation identifier with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>A new operation identifier with the additional properties</returns>
    public CertOperationId WithProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var builder = _properties.ToBuilder();
        foreach (var kvp in properties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value is not null)
            {
                builder[kvp.Key] = kvp.Value;
            }
        }

        return new CertOperationId(Id, Name, builder.ToImmutable(), CreatedUtc);
    }

    /// <summary>
    /// Creates a new operation identifier with a different name
    /// </summary>
    /// <param name="name">New operation name</param>
    /// <returns>A new operation identifier with the new name</returns>
    public CertOperationId WithName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return new CertOperationId(Id, name, _properties, CreatedUtc);
    }

    /// <summary>
    /// Gets a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <returns>Property value or default</returns>
    public T? GetPropertyValue<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _properties.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;
    }

    /// <summary>
    /// Tries to get a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <param name="value">Output property value</param>
    /// <returns>True if property exists and is of the correct type</returns>
    public bool TryGetPropertyValue<T>(string key, out T? value)
    {
        value = default;
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (_properties.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a property exists
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>True if the property exists</returns>
    public bool HasProperty(string key)
    {
        return !string.IsNullOrEmpty(key) && _properties.ContainsKey(key);
    }

    /// <summary>
    /// Returns a string representation of the operation ID
    /// </summary>
    public override string ToString()
    {
        return $"{Name}:{Id} [{CreatedUtc:O}]";
    }

    /// <summary>
    /// Determines if this operation ID is equal to another
    /// </summary>
    public bool Equals(CertOperationId? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(Id, other.Id, StringComparison.Ordinal) &&
               string.Equals(Name, other.Name, StringComparison.Ordinal) &&
               CreatedUtc.Equals(other.CreatedUtc);
    }

    /// <summary>
    /// Determines if this operation ID is equal to another object
    /// </summary>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is CertOperationId other && Equals(other));
    }

    /// <summary>
    /// Gets a hash code for the operation ID
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, CreatedUtc);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(CertOperationId? left, CertOperationId? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(CertOperationId? left, CertOperationId? right)
    {
        return !Equals(left, right);
    }
}
