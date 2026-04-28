// PiQApi.Abstractions/Core/Interfaces/IResource.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Represents a system resource with tracking and status information
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// Gets the unique resource identifier
        /// </summary>
        string ResourceId { get; }

        /// <summary>
        /// Gets the current status of the resource
        /// </summary>
        ResourceStatusType Status { get; }

        /// <summary>
        /// Gets the creation timestamp
        /// </summary>
        DateTime CreatedUtc { get; }

        /// <summary>
        /// Gets the last modification timestamp
        /// </summary>
        DateTime LastModifiedUtc { get; }

        /// <summary>
        /// Gets the collection of resource metadata
        /// </summary>
        IReadOnlyDictionary<string, object> Metadata { get; }
    }
}