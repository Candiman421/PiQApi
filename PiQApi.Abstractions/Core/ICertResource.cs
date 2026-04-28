// PiQApi.Abstractions/Core/ICertResource.cs

namespace PiQApi.Abstractions.Core;

/// <summary>
/// Interface for certificate resources
/// </summary>
public interface ICertResource
{
    /// <summary>
    /// Gets the unique identifier for this resource
    /// </summary>
    string ResourceId { get; }

    /// <summary>
    /// Gets the name of the resource
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of the resource
    /// </summary>
    string ResourceType { get; }

    /// <summary>
    /// Gets a value indicating whether this resource has been modified
    /// </summary>
    bool IsModified { get; }

    /// <summary>
    /// Gets a value indicating whether this resource has been deleted
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the last modified timestamp
    /// </summary>
    DateTimeOffset? ModifiedAt { get; }
}
