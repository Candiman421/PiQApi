// CertApi.Abstractions/Core/ICertTrackable.cs
namespace CertApi.Abstractions.Core;

/// <summary>
/// Represents an entity that can be tracked through the system
/// </summary>
public interface ICertTrackable
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
