// CertApi.Core/Context/CertOperationState.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Enums;

namespace CertApi.Core.Context;

/// <summary>
/// Implementation of operation state management
/// </summary>
public sealed class CertOperationState : ICertOperationState
{
    /// <summary>
    /// Creates a new instance of the operation state
    /// </summary>
    /// <param name="status">Initial status</param>
    public CertOperationState(OperationStatusType status = OperationStatusType.Created)
    {
        StartTime = DateTimeOffset.UtcNow;
        StateTimestamp = StartTime;
        Status = status;
    }

    /// <inheritdoc />
    public OperationStatusType Status { get; private set; }

    /// <inheritdoc />
    public ErrorCodeType? ErrorCode { get; private set; }

    /// <inheritdoc />
    public string? ErrorMessage { get; private set; }

    /// <inheritdoc />
    public bool IsInProgress => Status is OperationStatusType.Created or
                               OperationStatusType.Initializing or
                               OperationStatusType.Running or
                               OperationStatusType.Transitioning;

    /// <inheritdoc />
    public bool IsCompleted => Status is OperationStatusType.Done or
                             OperationStatusType.Failed or
                             OperationStatusType.Stopped or
                             OperationStatusType.TimedOut;

    /// <inheritdoc />
    public bool IsFailed => Status is OperationStatusType.Failed or
                          OperationStatusType.TimedOut;

    /// <inheritdoc />
    public bool IsSuccessful => Status == OperationStatusType.Done;

    /// <inheritdoc />
    public DateTimeOffset StartTime { get; }

    /// <inheritdoc />
    public DateTimeOffset? EndTime { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset StateTimestamp { get; private set; }

    /// <inheritdoc />
    public TimeSpan StateDuration => DateTimeOffset.UtcNow - StateTimestamp;

    /// <summary>
    /// Updates the state of the operation with optional error details
    /// </summary>
    /// <param name="status">New operation status</param>
    /// <param name="errorCode">ErrorInfo code if applicable</param>
    /// <param name="errorMessage">Error message if applicable</param>
    public void UpdateState(OperationStatusType status, ErrorCodeType? errorCode = null, string? errorMessage = null)
    {
        Status = status;
        StateTimestamp = DateTimeOffset.UtcNow;

        if (IsCompleted && !EndTime.HasValue)
        {
            EndTime = DateTimeOffset.UtcNow;
        }

        if (IsFailed)
        {
            ErrorCode = errorCode ?? ErrorCodeType.InternalServerError;
            ErrorMessage = errorMessage;
        }
    }

    /// <inheritdoc />
    public void TransitionToRunning()
    {
        UpdateState(OperationStatusType.Running);
    }

    /// <inheritdoc />
    public void TransitionToComplete()
    {
        UpdateState(OperationStatusType.Done);
    }

    /// <inheritdoc />
    public void TransitionToFailed(ErrorCodeType errorCode = ErrorCodeType.InternalServerError)
    {
        UpdateState(OperationStatusType.Failed, errorCode);
    }

    /// <inheritdoc />
    public void TransitionToFailed(ErrorCodeType errorCode, string errorMessage)
    {
        UpdateState(OperationStatusType.Failed, errorCode, errorMessage);
    }

    /// <inheritdoc />
    public bool ValidateTransition(OperationStatusType newStatus)
    {
        // Basic validation to prevent obviously invalid transitions
        if (IsCompleted && newStatus != OperationStatusType.Created)
        {
            return false;
        }

        // Allow all other transitions for now
        return true;
    }
}