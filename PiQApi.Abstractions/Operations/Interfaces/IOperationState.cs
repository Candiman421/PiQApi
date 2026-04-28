// PiQApi.Abstractions/Operations/Interfaces/IOperationState.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Operations.Interfaces
{
    /// <summary>
    /// Represents the state of an operation
    /// </summary>
    public interface IOperationState
    {
        /// <summary>
        /// Gets the current status of the operation
        /// </summary>
        ServiceOperationStatusType Status { get; }

        /// <summary>
        /// Gets whether the operation is active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets whether the operation has completed
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets whether the operation has failed
        /// </summary>
        bool IsFailed { get; }

        /// <summary>
        /// Gets whether the operation was successful
        /// </summary>
        bool IsSuccessful { get; }

        /// <summary>
        /// Gets the timestamp when the operation started
        /// </summary>
        DateTimeOffset StartTime { get; }

        /// <summary>
        /// Gets the timestamp when the operation completed, if applicable
        /// </summary>
        DateTimeOffset? EndTime { get; }

        /// <summary>
        /// Gets the error code if the operation failed
        /// </summary>
        ErrorCodeType? ErrorCode { get; }

        /// <summary>
        /// Updates the operation state
        /// </summary>
        /// <param name="status">New operation status</param>
        /// <param name="errorCode">Error code, if applicable</param>
        void UpdateState(ServiceOperationStatusType status, ErrorCodeType? errorCode = null);
    }
}