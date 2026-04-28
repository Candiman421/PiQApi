// PiQApi.Abstractions/Context/IPiQOperationState.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Context;

/// <summary>
/// Manages state for service operations
/// </summary>
public interface IPiQOperationState
{
    /// <summary>
    /// Gets the current operation status
    /// </summary>
    OperationStatusType Status { get; }

    /// <summary>
    /// Gets the error code if the operation failed
    /// </summary>
    ErrorCodeType? ErrorCode { get; }
    
    /// <summary>
    /// Gets the error message if the operation failed
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Gets a value indicating whether the operation is in progress
    /// </summary>
    bool IsInProgress { get; }

    /// <summary>
    /// Gets a value indicating whether the operation has completed
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets a value indicating whether the operation has failed
    /// </summary>
    bool IsFailed { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful
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
    /// Gets the timestamp when the current state was set
    /// </summary>
    DateTimeOffset StateTimestamp { get; }

    /// <summary>
    /// Gets the duration in the current state
    /// </summary>
    TimeSpan StateDuration { get; }

    /// <summary>
    /// Updates the state of the operation
    /// </summary>
    /// <param name="status">New operation status</param>
    /// <param name="errorCode">ErrorInfo code if applicable</param>
    /// <param name="errorMessage">Error message if applicable</param>
    void UpdateState(OperationStatusType status, ErrorCodeType? errorCode = null, string? errorMessage = null);

    /// <summary>
    /// Transitions the operation to the running state
    /// </summary>
    void TransitionToRunning();

    /// <summary>
    /// Transitions the operation to the completed state
    /// </summary>
    void TransitionToComplete();

    /// <summary>
    /// Transitions the operation to the failed state
    /// </summary>
    /// <param name="errorCode">ErrorInfo code to associate with the failure</param>
    void TransitionToFailed(ErrorCodeType errorCode = ErrorCodeType.InternalServerError);

    /// <summary>
    /// Transitions the operation to the failed state with a detailed error message
    /// </summary>
    /// <param name="errorCode">ErrorInfo code to associate with the failure</param>
    /// <param name="errorMessage">Detailed error message explaining the failure</param>
    void TransitionToFailed(ErrorCodeType errorCode, string errorMessage);

    /// <summary>
    /// Validates the current state against a transition to the specified state
    /// </summary>
    /// <param name="newStatus">Proposed new status</param>
    /// <returns>True if the transition is valid</returns>
    bool ValidateTransition(OperationStatusType newStatus);
}