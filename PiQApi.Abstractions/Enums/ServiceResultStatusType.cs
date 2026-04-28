// PiQApi.Abstractions/Enums/ServiceResultStatusType.cs
namespace PiQApi.Abstractions.Enums
{
    /// <summary>
    /// Represents the current status of a service operation
    /// </summary>
    public enum ServiceResultStatusType
    {
        /// <summary>
        /// Operation completed successfully
        /// </summary>
        Success = 0,

        /// <summary>
        /// Operation completed with partial success
        /// </summary>
        Partial = 1,

        /// <summary>
        /// Operation is in initial state
        /// </summary>
        Created = 2,

        /// <summary>
        /// Operation is currently initializing
        /// </summary>
        Initializing = 3,

        /// <summary>
        /// Operation is in progress
        /// </summary>
        Running = 4,

        /// <summary>
        /// Operation is temporarily suspended
        /// </summary>
        Suspended = 5,

        /// <summary>
        /// Operation is resuming from suspended state
        /// </summary>
        Resuming = 6,

        /// <summary>
        /// Operation exceeded time limit
        /// </summary>
        TimedOut = 7,

        /// <summary>
        /// Operation has been stopped
        /// </summary>
        Stopped = 8,

        /// <summary>
        /// Operation completed with validation failures
        /// </summary>
        ValidationFailed = 9,

        /// <summary>
        /// Operation encountered an unrecoverable error
        /// </summary>
        Failed = 10
    }
}