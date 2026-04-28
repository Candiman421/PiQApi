// PiQApi.Abstractions/Enums/OperationStatusType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Represents the lifecycle state of an operation
/// </summary>
public enum OperationStatusType
{
    /// <summary>
    /// Operation is in initial state
    /// </summary>
    Created = 0,

    /// <summary>
    /// Operation is currently initializing
    /// </summary>
    Initializing = 1,

    /// <summary>
    /// Operation is in progress
    /// </summary>
    Running = 2,

    /// <summary>
    /// Operation completed successfully
    /// </summary>
    Done = 3,

    /// <summary>
    /// Operation encountered an error
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Operation was manually stopped
    /// </summary>
    Stopped = 5,

    /// <summary>
    /// Operation exceeded time limit
    /// </summary>
    TimedOut = 6,

    /// <summary>
    /// Operation is temporarily suspended
    /// </summary>
    Suspended = 7,

    /// <summary>
    /// Operation is resuming from suspended state
    /// </summary>
    Resuming = 8,

    /// <summary>
    /// Operation was canceled
    /// </summary>
    Canceled = 9,

    /// <summary>
    /// Operation is in transition between states
    /// </summary>
    Transitioning = 10
}