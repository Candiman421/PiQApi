// PiQApi.Abstractions/Core/Models/ResourceId.cs
namespace PiQApi.Abstractions.Core.Models
{
    /// <summary>
    /// Represents a unique identifier for a resource
    /// </summary>
    public sealed class ResourceId : IEquatable<ResourceId?>
    {
        /// <summary>
        /// Gets the resource identifier value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the resource type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the creation timestamp
        /// </summary>
        public DateTime CreatedUtc { get; }

        /// <summary>
        /// Creates a new resource identifier
        /// </summary>
        /// <param name="type">Resource type</param>
        /// <param name="value">Resource identifier value</param>
        public ResourceId(string type, string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(type);
            ArgumentException.ThrowIfNullOrEmpty(value);

            Type = type;
            Value = value;
            CreatedUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Tries to parse a string into a resource identifier
        /// </summary>
        /// <param name="input">Input string in format "type:value"</param>
        /// <param name="resourceId">Output resource identifier</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool TryParse(string? input, out ResourceId? resourceId)
        {
            resourceId = null;

            if (string.IsNullOrEmpty(input))
                return false;

            // Check for the first colon
            int colonIndex = input.IndexOf(':', StringComparison.Ordinal);
            if (colonIndex <= 0 || colonIndex == input.Length - 1)
                return false;

            string type = input[..colonIndex];
            string value = input[(colonIndex + 1)..];

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
                return false;

            try
            {
                resourceId = new ResourceId(type, value);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a string representation of the resource ID
        /// </summary>
        public override string ToString()
        {
            return $"{Type}:{Value}";
        }

        /// <summary>
        /// Determines if this resource ID is equal to another
        /// </summary>
        /// <param name="other">Other resource ID</param>
        /// <returns>True if the resource IDs are equal</returns>
        public bool Equals(ResourceId? other)
        {
            return other is not null
                         && (ReferenceEquals(this, other)
                         || (string.Equals(Value, other.Value, StringComparison.Ordinal) &&
                            string.Equals(Type, other.Type, StringComparison.Ordinal)));
        }

        /// <summary>
        /// Determines if this resource ID is equal to another object
        /// </summary>
        /// <param name="obj">Other object</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is ResourceId other && Equals(other));
        }

        /// <summary>
        /// Gets a hash code for the resource ID
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Type);
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(ResourceId? left, ResourceId? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(ResourceId? left, ResourceId? right)
        {
            return !Equals(left, right);
        }
    }
}