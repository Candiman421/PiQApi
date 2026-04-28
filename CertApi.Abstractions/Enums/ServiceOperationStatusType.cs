// CertApi.Abstractions/Enums/ServiceOperationStatusType.cs
namespace CertApi.Abstractions.Enums
{
    /// <summary>
    /// Represents the current status of a service operation
    /// </summary>
    public enum ServiceOperationStatusType
    {
        /// <summary>
        /// Operation is complete and successful
        /// </summary>
        Done = 0,

        /// <summary>
        /// Operation requires additional resources or processing
        /// </summary>
        PartiallyComplete = 1,

        /// <summary>
        /// Operation needs more data or context to complete
        /// </summary>
        Pending = 2,

        /// <summary>
        /// Operation encountered an unrecoverable error
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Operation is in initial state
        /// </summary>
        Created = 4,

        /// <summary>
        /// Operation is currently initializing
        /// </summary>
        Initializing = 5,

        /// <summary>
        /// Operation is in progress
        /// </summary>
        Running = 6,

        /// <summary>
        /// Operation is transitioning between states
        /// </summary>
        Transitioning = 7,

        /// <summary>
        /// Operation is temporarily suspended
        /// </summary>
        Suspended = 8,

        /// <summary>
        /// Operation is resuming from suspended state
        /// </summary>
        Resuming = 9,

        /// <summary>
        /// Operation exceeded time limit
        /// </summary>
        TimedOut = 10,

        /// <summary>
        /// Operation is in the process of stopping
        /// </summary>
        Stopping = 11,

        /// <summary>
        /// Operation has been stopped
        /// </summary>
        Stopped = 12
    }
}