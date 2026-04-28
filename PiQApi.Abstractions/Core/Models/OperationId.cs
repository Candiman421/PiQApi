// PiQApi.Abstractions/Core/Models/OperationId.cs
namespace PiQApi.Abstractions.Core.Models
{
    /// <summary>
    /// Represents a unique identifier for an operation
    /// </summary>
    public sealed class OperationId : IEquatable<OperationId?>
    {
        /// <summary>
        /// Gets the unique identifier
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the parent operation identifier, if any
        /// </summary>
        public OperationId? ParentId { get; }

        /// <summary>
        /// Gets the correlation identifier
        /// </summary>
        public CorrelationId CorrelationId { get; }

        /// <summary>
        /// Gets the creation timestamp
        /// </summary>
        public DateTime CreatedUtc { get; }

        /// <summary>
        /// Creates a new operation identifier
        /// </summary>
        /// <param name="id">The operation identifier</param>
        /// <param name="correlationId">Correlation identifier</param>
        /// <param name="parentId">Optional parent operation identifier</param>
        public OperationId(string id, CorrelationId correlationId, OperationId? parentId = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(id);
            ArgumentNullException.ThrowIfNull(correlationId);

            Id = id;
            CorrelationId = correlationId;
            ParentId = parentId;
            CreatedUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a string representation of the operation ID
        /// </summary>
        public override string ToString()
        {
            return ParentId != null ? $"{ParentId.Id}/{Id}" : Id;
        }

        /// <summary>
        /// Determines if this operation ID is equal to another
        /// </summary>
        /// <param name="other">Other operation ID</param>
        /// <returns>True if the operation IDs are equal</returns>
        public bool Equals(OperationId? other)
        {
            return other is not null && (ReferenceEquals(this, other) || string.Equals(Id, other.Id, StringComparison.Ordinal));
        }

        /// <summary>
        /// Determines if this operation ID is equal to another object
        /// </summary>
        /// <param name="obj">Other object</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is OperationId other && Equals(other));
        }

        /// <summary>
        /// Gets a hash code for the operation ID
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode(StringComparison.Ordinal);
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(OperationId? left, OperationId? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(OperationId? left, OperationId? right)
        {
            return !Equals(left, right);
        }
    }
}