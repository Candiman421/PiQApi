// PiQApi.Abstractions/Core/Interfaces/ICorrelationContext.cs
namespace PiQApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Provides context for tracking operations across system boundaries
    /// </summary>
    public interface ICorrelationContext
    {
        /// <summary>
        /// Gets the unique correlation identifier
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Gets the creation timestamp
        /// </summary>
        DateTime CreatedUtc { get; }

        /// <summary>
        /// Gets the parent correlation identifier, if any
        /// </summary>
        string? ParentCorrelationId { get; }

        /// <summary>
        /// Gets the collection of correlation properties
        /// </summary>
        IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        /// Adds a property to the correlation context
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        void AddProperty(string key, object value);

        /// <summary>
        /// Adds multiple properties to the correlation context
        /// </summary>
        /// <param name="properties">Properties to add</param>
        /// <exception cref="ArgumentNullException">Thrown when properties is null</exception>
        void AddProperties(IDictionary<string, object> properties);

        /// <summary>
        /// Gets a property value
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="key">Property key</param>
        /// <returns>Property value</returns>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when property doesn't exist</exception>
        /// <exception cref="InvalidCastException">Thrown when property exists but can't be cast to T</exception>
        T GetProperty<T>(string key);

        /// <summary>
        /// Tries to get a property value
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="key">Property key</param>
        /// <param name="value">Output property value</param>
        /// <returns>True if property exists and can be cast to T; otherwise, false</returns>
        bool TryGetProperty<T>(string key, out T? value);

        /// <summary>
        /// Sets the parent correlation identifier
        /// </summary>
        /// <param name="parentId">Parent correlation identifier</param>
        /// <exception cref="ArgumentException">Thrown when parentId is null or empty</exception>
        void SetParentCorrelation(string parentId);

        /// <summary>
        /// Checks if a property exists
        /// </summary>
        /// <param name="key">Property key</param>
        /// <returns>True if the property exists; otherwise, false</returns>
        bool HasProperty(string key);
    }
}