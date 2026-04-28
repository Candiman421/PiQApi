// CertApi.Core/Operations/CertOperationBase.Lifecycle.cs
using CertApi.Abstractions.Enums;

namespace CertApi.Core.Operations;

public abstract partial class CertOperationBase : IAsyncDisposable
{
    /// <summary>
    /// Initializes the operation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(CertOperationBase));

        var lockReleaser = await _asyncLock.AcquireAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();

            if (IsReady)
            {
                return;
            }

            using var scope = CreateLoggingScope();
            LogInitializing(_logger, OperationId, null);

            // Start tracking metrics with correlation ID
            _operationStartTime = _timeProvider.GetTimestamp();
            string totalMetric = "OperationTotal";
            string corrTotalMetric = $"{totalMetric}_{CorrelationId}";
            string initMetric = "Initialization";
            string corrInitMetric = $"{initMetric}_{CorrelationId}";

            Context.Metrics.StartTimer(totalMetric);
            Context.Metrics.IncrementCounter(corrTotalMetric);
            Context.Metrics.StartTimer(initMetric);
            Context.Metrics.IncrementCounter(corrInitMetric);

            try
            {
                // Initialize the context
                await Context.InitializeAsync(cancellationToken).ConfigureAwait(false);

                // Log operation start
                await Context.LogOperationStartAsync().ConfigureAwait(false);

                // Add metrics
                TimeSpan initTime = Context.Metrics.StopTimer(initMetric);
                Context.CorrelationContext.AddProperty("InitializationTimeMs", initTime.TotalMilliseconds);
                Context.Metrics.RecordTime($"{corrInitMetric}_Duration", initTime);

                // Update state
                Context.State.TransitionToRunning();

                // Mark as ready
                IsReady = true;
            }
            catch (Exception ex)
            {
                // Add correlation ID to exception
                if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationId))
                {
                    ex.Data["CorrelationId"] = CorrelationId;
                    ex.Data["OperationId"] = OperationId;
                }

                await LogErrorAsync(ex).ConfigureAwait(false);
                Context.State.TransitionToFailed(ErrorCodeType.InternalServerError);
                throw;
            }
        }
        finally
        {
            lockReleaser.Dispose();
        }
    }

    /// <summary>
    /// Cleans up any resources used by the operation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public virtual async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            return;
        }

        var lockReleaser = await _asyncLock.AcquireAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();

            using var scope = CreateLoggingScope();

            // Log cleanup operation start
            LogCleaning(_logger, OperationId, null);

            string cleanupMetric = "Cleanup";
            string corrCleanupMetric = $"{cleanupMetric}_{CorrelationId}";

            Context.Metrics.StartTimer(cleanupMetric);
            Context.Metrics.IncrementCounter(corrCleanupMetric);

            try
            {
                // Mark the operation as completed if it was still running
                if (Context.State.IsInProgress)
                {
                    Context.State.TransitionToComplete();
                }

                // Record success metrics if the operation wasn't failed
                if (Context.State.Status != OperationStatusType.Failed)
                {
                    Context.Metrics.IncrementCounter("Operation_Success");
                    Context.Metrics.IncrementCounter($"Operation_Success_{CorrelationId}");
                }
            }
            catch (Exception ex)
            {
                // Add correlation ID to exception
                if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationId))
                {
                    ex.Data["CorrelationId"] = CorrelationId;
                    ex.Data["OperationId"] = OperationId;
                }

                await LogErrorAsync(ex).ConfigureAwait(false);
                // Don't re-throw as this is cleanup and should continue
            }
            finally
            {
                // Record cleanup time
                TimeSpan cleanupTime = Context.Metrics.StopTimer(cleanupMetric);
                Context.CorrelationContext.AddProperty("CleanupTimeMs", cleanupTime.TotalMilliseconds);
                Context.Metrics.RecordTime($"{corrCleanupMetric}_Duration", cleanupTime);
            }
        }
        finally
        {
            lockReleaser.Dispose();
        }
    }

    /// <summary>
    /// Validates the operation's current state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public virtual async Task ValidateStateAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(CertOperationBase));

        var lockReleaser = await _asyncLock.AcquireAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();

            using var scope = CreateLoggingScope();
            LogValidatingState(_logger, OperationId, null);

            // Start validation metrics with correlation ID
            string validationMetric = "Validation";
            string corrValidationMetric = $"{validationMetric}_{CorrelationId}";
            Context.Metrics.StartTimer(validationMetric);
            Context.Metrics.IncrementCounter(corrValidationMetric);

            try
            {
                if (!IsReady)
                {
                    throw new InvalidOperationException($"Operation {OperationId} is not initialized. Call InitializeAsync first. CorrelationId: {CorrelationId}");
                }

                // Validate the context
                await Context.ValidateAsync(cancellationToken).ConfigureAwait(false);

                // Check if operation is in a valid state
                if (!Context.State.IsInProgress)
                {
                    throw new InvalidOperationException($"Operation {OperationId} is in an invalid state: {Context.State.Status}. CorrelationId: {CorrelationId}");
                }
            }
            finally
            {
                // Record validation time
                TimeSpan validationTime = Context.Metrics.StopTimer(validationMetric);
                Context.CorrelationContext.AddProperty("ValidationTimeMs", validationTime.TotalMilliseconds);
                Context.Metrics.RecordTime($"{corrValidationMetric}_Duration", validationTime);
            }
        }
        finally
        {
            lockReleaser.Dispose();
        }
    }

    /// <summary>
    /// Disposes the operation asynchronously
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        IDisposable? lockReleaser = null;
        try
        {
            // Attempt to acquire the lock, but don't wait too long
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try
            {
                var lockTask = _asyncLock.AcquireAsync(cts.Token);
                // Only wait for a short time to avoid hanging
                lockReleaser = await lockTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // If we couldn't acquire the lock, continue with disposal anyway
                LogLockAcquisitionFailed(_logger, OperationId, CorrelationId, null);
            }
            catch (ObjectDisposedException)
            {
                // Lock was already disposed, which is fine here
            }

            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            // Ensure cleanup is performed
            try
            {
                await CleanupAsync(cts.Token).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Cleanup exceptions shouldn't prevent disposal
            }

            // Record total operation time with correlation ID
            long durationMs = 0;

            if (_operationStartTime.HasValue)
            {
                TimeSpan duration = _timeProvider.GetElapsedTime(_operationStartTime.Value);
                durationMs = (long)duration.TotalMilliseconds;
                Context.CorrelationContext.AddProperty("TotalDurationMs", durationMs);
            }

            // Also stop the metrics timer if it was started
            try
            {
                string totalMetric = "OperationTotal";
                string corrTotalMetric = $"{totalMetric}_{CorrelationId}";
                TimeSpan duration = Context.Metrics.StopTimer(totalMetric);
                Context.CorrelationContext.AddProperty("OperationTotalTimerMs", duration.TotalMilliseconds);
                Context.Metrics.RecordTime($"{corrTotalMetric}_Duration", duration);
            }
            catch (Exception)
            {
                // CA1031 justification: Metrics stopping is non-critical during disposal.
                // We don't want to prevent completion of the disposal process if metrics
                // operations fail.
                // Ignore errors stopping metrics - timer might not have been started
            }

            // Log operation end if it's still active
            if (IsActive)
            {
                try
                {
                    bool success = Context.State.Status != OperationStatusType.Failed;
                    await Context.LogOperationEndAsync(success).ConfigureAwait(false);

                    if (success)
                    {
                        LogSuccess();
                    }
                    else
                    {
                        LogFailure();
                    }
                }
                catch (Exception ex)
                {
                    // CA1031 justification: Error logging during disposal should not
                    // prevent completion of the disposal process.
                    // Add correlation ID to exception
                    if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationId))
                    {
                        ex.Data["CorrelationId"] = CorrelationId;
                    }

                    // Use LoggerMessage delegate
                    LogOperationEndError(_logger, OperationId, CorrelationId, ex);
                }
            }

            // Dispose the operation context if it implements IAsyncDisposable
            if (Context is IAsyncDisposable disposableContext)
            {
                await disposableContext.DisposeAsync().ConfigureAwait(false);
            }

            // Log completion
            LogDisposeCompleted(_logger, OperationId, durationMs, null);
        }
        finally
        {
            // Dispose the lock releaser if we acquired it
            lockReleaser?.Dispose();

            // Make sure to dispose the async lock
            await _asyncLock.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
