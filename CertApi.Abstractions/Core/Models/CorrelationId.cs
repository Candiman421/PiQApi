// CertApi.Abstractions/Core/Models/CorrelationId.cs
namespace CertApi.Abstractions.Core.Models
{
    /// <summary>
    /// Represents a correlation identifier for tracing operations across system boundaries
    /// </summary>
    public sealed class CorrelationId
    {
        private readonly Dictionary<string, object> _properties = new(capacity: 4);

        /// <summary>
        /// Gets the unique identifier
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the collection of correlation properties
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties => _properties;

        /// <summary>
        /// Gets the creation timestamp
        /// </summary>
        public DateTime CreatedUtc { get; }

        /// <summary>
        /// Creates a new correlation identifier
        /// </summary>
        /// <param name="id">The correlation identifier</param>
        public CorrelationId(string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id);
            Id = id;
            CreatedUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a property to the correlation context
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void AddProperty(string key, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            lock (_properties)
            {
                _properties[key] = value;
            }
        }

        /// <summary>
        /// Adds multiple properties to the correlation context
        /// </summary>
        /// <param name="properties">Properties to add</param>
        public void AddProperties(IDictionary<string, object> properties)
        {
            ArgumentNullException.ThrowIfNull(properties);

            lock (_properties)
            {
                foreach (KeyValuePair<string, object> property in properties)
                {
                    _properties[property.Key] = property.Value;
                }
            }
        }

        /// <summary>
        /// Gets a property value
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="key">Property key</param>
        /// <returns>Property value or default</returns>
        public T? GetProperty<T>(string key)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);

            lock (_properties)
            {
                return _properties.TryGetValue(key, out object? value) && value is T typedValue ? typedValue : default;
            }
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

            lock (_properties)
            {
                if (_properties.TryGetValue(key, out object? obj) && obj is T typedValue)
                {
                    value = typedValue;
                    return true;
                }
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
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            lock (_properties)
            {
                return _properties.ContainsKey(key);
            }
        }

        /// <summary>
        /// Returns a string representation of the correlation ID
        /// </summary>
        public override string ToString()
        {
            return $"{Id} [{CreatedUtc:O}]";
        }
    }
}