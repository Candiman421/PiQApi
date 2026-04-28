// PiQApi.Abstractions/Enums/ResourceStatusType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Represents the current status of a managed resource in the system
/// </summary>
public enum ResourceStatusType
{
    /// <summary>
    /// Resource is available for use and properly initialized
    /// </summary>
    Available = 0,

    /// <summary>
    /// Resource is currently in use by an operation
    /// </summary>
    InUse = 1,

    /// <summary>
    /// Resource is locked and cannot be accessed
    /// </summary>
    Locked = 2,

    /// <summary>
    /// Resource is in an error state
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Resource is being initialized
    /// </summary>
    Initializing = 4,

    /// <summary>
    /// Resource is being disposed or cleaned up
    /// </summary>
    Disposing = 5,

    /// <summary>
    /// Resource has been disposed and is no longer usable
    /// </summary>
    Disposed = 6,

    /// <summary>
    /// Resource is temporarily unavailable but may become available
    /// </summary>
    Unavailable = 7,

    /// <summary>
    /// Resource requires maintenance or repair
    /// </summary>
    NeedsMaintenance = 8,

    /// <summary>
    /// Resource is being created or allocated
    /// </summary>
    Creating = 9,

    /// <summary>
    /// Resource is in an unknown state
    /// </summary>
    Unknown = 10
}