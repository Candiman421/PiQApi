// PiQApi.Core/Context/PiQOperationContext.Lifecycle.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Context;

public partial class PiQOperationContext
{
    #region LoggerMessage Delegates
    // Lifecycle-specific logging delegates
    private static readonly Action<ILogger, string, Exception?> LogInitializationFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(30, nameof(InitializeAsync)),
            "Failed to initialize operation {OperationId}");

    private static readonly Action<ILogger, string, Exception?> LogCompletionFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(31, nameof(CompleteAsync)),
            "Failed to complete operation {OperationId}");
    #endregion

    /// <summary>
    /// Initializes the operation context
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();

            // Add basic operation information to correlation context
            CorrelationContext.AddProperty("OperationId", Identifier.Id);
            CorrelationContext.AddProperty("OperationName", Identifier.Name);
            CorrelationContext.AddProperty("OperationType", Identifier.OperationType.ToString());

            // Start tracking operation time
            Metrics.StartTimer("OperationDuration");

            // Add initialization timestamp to properties
            AddProperty("InitializedUtc", DateTimeOffset.UtcNow);

            // Set state to initializing
            UpdateState(OperationStatusType.Initializing);

            try
            {
                // Allow derived classes to perform custom initialization
                await OnInitializeAsync(cancellationToken).ConfigureAwait(false);

                // Update state to running (initialized state)
                UpdateState(OperationStatusType.Running);

                // Log operation initialization
                LogOperationInitialized(_logger, Identifier.Id, Identifier.OperationType,
                    CorrelationContext.CorrelationId, null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Update state to failed
                UpdateState(OperationStatusType.Failed, ErrorCodeType.InvalidOperation);

                // Log failure using LoggerMessage delegate
                LogInitializationFailed(_logger, Identifier.Id, ex);
                throw;
            }
        }
        finally
        {
            _ = _asyncLock.Release();
        }
    }

    /// <summary>
    /// Updates the operation state
    /// </summary>
    /// <param name="status">New operation status</param>
    /// <param name="errorCode">Optional error code</param>
    public void UpdateState(OperationStatusType status, ErrorCodeType? errorCode = null)
    {
        ThrowIfDisposed();

        if (State is PiQOperationState state)
        {
            state.UpdateState(status, errorCode);
        }
    }

    /// <summary>
    /// Validates the operation context
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (Identifier == null)
            {
                throw new InvalidOperationException("Operation identifier is required");
            }

            if (CorrelationContext == null)
            {
                throw new InvalidOperationException("Correlation context is required");
            }

            // Record validation in metrics
            Metrics.RecordTime("ValidationTime", TimeSpan.FromMilliseconds(1));

            LogOperationValidated(_logger, Identifier.Id, null);
        }
        finally
        {
            _ = _asyncLock.Release();
        }
    }

    /// <summary>
    /// Completes the operation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();

            if (State.IsCompleted)
            {
                return;
            }

            Metrics.StartTimer("Complete");

            try
            {
                // Set state to transitioning (completing state)
                UpdateState(OperationStatusType.Transitioning);

                // Allow derived classes to perform custom completion
                await OnCompleteAsync(cancellationToken).ConfigureAwait(false);

                // Set state to done (completed state)
                UpdateState(OperationStatusType.Done);

                Metrics.StopTimer("Complete");

                // Log completion
                LogOperationCompleted(_logger, Identifier.Id,
                    Metrics.GetTimerElapsed("OperationDuration").TotalMilliseconds,
                    CorrelationContext.CorrelationId, null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                UpdateState(OperationStatusType.Failed, ErrorCodeType.ServiceError);

                // Log failure using LoggerMessage delegate
                LogCompletionFailed(_logger, Identifier.Id, ex);
                throw;
            }
        }
        finally
        {
            _ = _asyncLock.Release();
        }
    }

    /// <summary>
    /// Creates a child operation context with the specified name
    /// </summary>
    /// <param name="operationName">Name of the child operation</param>
    /// <returns>A new operation context with this context as parent</returns>
    public IPiQOperationContext CreateChildContext(string operationName)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));
        ThrowIfDisposed();

        // Create child identifier
        var childId = new PiQOperationIdentifier(
            Guid.NewGuid().ToString(),
            operationName,
            Identifier.OperationType,
            Identifier,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(5));

        // Create child context with proper implementation
        var childContext = new PiQOperationContext(
            childId,
            new PiQOperationState(),
            new PiQOperationMetrics(childId.Id, operationName),
            CorrelationContext,
            _validator,
            _resources,
            _logger,
            CancellationToken);

        // Log child context creation
        LogChildContextCreated(_logger, childId.Id, Identifier.Id, null);

        return childContext;
    }

    /// <summary>
    /// Creates a scope for the current operation
    /// </summary>
    /// <param name="scopeName">Name of the scope</param>
    /// <returns>A disposable scope that will record metrics when disposed</returns>
    public IDisposable CreateScope(string scopeName)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(scopeName, nameof(scopeName));

        // Start timing the scope
        Metrics.StartTimer(scopeName);

        // Log scope creation
        LogScopeStarted(_logger, scopeName, Identifier.Id, null);

        // Create a new scope with the current context
        return new PiQOperationScope(scopeName, this, _logger, Metrics);
    }

    /// <summary>
    /// Called during initialization. Override in derived classes to add custom initialization.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called during completion. Override in derived classes to add custom completion logic.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    protected virtual Task OnCompleteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}