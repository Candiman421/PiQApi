// CertApi.Core/Core/Models/CertCorrelationId.cs
using System.Collections.Immutable;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Utilities.Time;
using CertApi.Core.Utilities.Time;

namespace CertApi.Core.Core.Models;

/// <summary>
/// Represents a correlation identifier for tracing operations across system boundaries
/// </summary>
public sealed class CertCorrelationId : ICertCorrelationId, IEquatable<CertCorrelationId>
{
    private readonly ImmutableDictionary<string, object> _properties;
    private readonly ICertTimeProvider _timeProvider;

    /// <summary>
    /// Gets the unique identifier
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the unique identifier value (alias for Id)
    /// </summary>
    public string Value => Id;

    /// <summary>
    /// Gets the collection of correlation properties
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTime CreatedUtc { get; }

    /// <summary>
    /// Creates a new correlation identifier with just the ID
    /// Uses the current time provider from the factory
    /// </summary>
    /// <param name="id">The correlation identifier</param>
    /// <exception cref="ArgumentException">Thrown when id is null or empty</exception>
    public CertCorrelationId(string id)
        : this(id, CertTimeProviderFactory.Current)
    {
    }

    /// <summary>
    /// Creates a new correlation identifier
    /// </summary>
    /// <param name="id">The correlation identifier</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    /// <exception cref="ArgumentException">Thrown when id is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when timeProvider is null</exception>
    public CertCorrelationId(string id, ICertTimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(timeProvider);

        // Now call the full constructor with default values
        Id = id;
        _properties = ImmutableDictionary<string, object>.Empty;
        CreatedUtc = timeProvider.UtcNow;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Creates a new correlation identifier with specified properties and creation time
    /// </summary>
    /// <param name="id">The correlation identifier</param>
    /// <param name="properties">Properties dictionary</param>
    /// <param name="createdUtc">Creation timestamp</param>
    /// <param name="timeProvider">Time provider instance</param>
    /// <exception cref="ArgumentException">Thrown when id is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when timeProvider is null</exception>
    public CertCorrelationId(string id, ImmutableDictionary<string, object> properties, DateTime createdUtc, ICertTimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(timeProvider);

        Id = id;
        _properties = properties ?? ImmutableDictionary<string, object>.Empty;
        CreatedUtc = createdUtc;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Creates a new correlation identifier with an additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>A new correlation identifier with the additional property</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public ICertCorrelationId WithProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
        return new CertCorrelationId(Id, _properties.SetItem(key, value), CreatedUtc, _timeProvider);
    }

    /// <summary>
    /// Creates a new correlation identifier with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>A new correlation identifier with the additional properties</returns>
    /// <exception cref="ArgumentNullException">Thrown when properties is null</exception>
    public ICertCorrelationId WithProperties(IDictionary<string, object> properties)
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

        return new CertCorrelationId(Id, builder.ToImmutable(), CreatedUtc, _timeProvider);
    }

    /// <summary>
    /// Gets a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <returns>Property value or default</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    public T? GetProperty<T>(string key)
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
    public bool TryGetProperty<T>(string key, out T? value)
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
    /// Returns a string representation of the correlation ID
    /// </summary>
    public override string ToString()
    {
        return Id;
    }

    /// <summary>
    /// Creates a concrete CertCorrelationId from an ICertCorrelationId interface
    /// </summary>
    /// <param name="correlationId">The correlation ID interface</param>
    /// <returns>A concrete CertCorrelationId implementation</returns>
    public static CertCorrelationId FromInterface(ICertCorrelationId correlationId)
    {
        ArgumentNullException.ThrowIfNull(correlationId);

        // If it's already a CertCorrelationId, just return it
        if (correlationId is CertCorrelationId concreteId)
        {
            return concreteId;
        }

        // Otherwise, create a new one with all the properties from the interface
        var propertiesDict = new Dictionary<string, object>();
        foreach (var prop in correlationId.Properties)
        {
            propertiesDict[prop.Key] = prop.Value;
        }

        var immutableProps = propertiesDict.Count > 0
            ? ImmutableDictionary.CreateRange(propertiesDict)
            : ImmutableDictionary<string, object>.Empty;

        return new CertCorrelationId(
            correlationId.Id,
            immutableProps,
            correlationId.CreatedUtc,
            CertTimeProviderFactory.Current);
    }

    /// <summary>
    /// Determines if this correlation ID is equal to another
    /// </summary>
    /// <param name="other">The CertCorrelationId to compare with</param>
    /// <returns>True if the correlation IDs are equal</returns>
    public bool Equals(CertCorrelationId? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(Id, other.Id, StringComparison.Ordinal) &&
               CreatedUtc.Equals(other.CreatedUtc);
    }

    /// <summary>
    /// Determines if this correlation ID is equal to another object
    /// </summary>
    /// <param name="obj">The object to compare with</param>
    /// <returns>True if the objects are equal</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is CertCorrelationId other && Equals(other));
    }

    /// <summary>
    /// Gets a hash code for the correlation ID
    /// </summary>
    /// <returns>A hash code</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, CreatedUtc);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(CertCorrelationId? left, CertCorrelationId? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(CertCorrelationId? left, CertCorrelationId? right)
    {
        return !Equals(left, right);
    }
}