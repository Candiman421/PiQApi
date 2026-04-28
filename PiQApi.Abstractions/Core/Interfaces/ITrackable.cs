// PiQApi.Abstractions/Core/Interfaces/ITrackable.cs
namespace PiQApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Represents an entity that can be tracked through the system
    /// </summary>
    public interface ITrackable
    {
        /// <summary>
        /// Gets the unique tracking identifier
        /// </summary>
        string TrackingId { get; }

        /// <summary>
        /// Gets the parent tracking identifier, if any
        /// </summary>
        string? ParentTrackingId { get; }

        /// <summary>
        /// Gets the collection of tracking metadata
        /// </summary>
        IReadOnlyDictionary<string, object> TrackingMetadata { get; }

        /// <summary>
        /// Gets the timestamp when tracking began
        /// </summary>
        DateTime TrackedUtc { get; }
    }
}