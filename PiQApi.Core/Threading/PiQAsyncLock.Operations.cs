// PiQApi.Core/Threading/PiQAsyncLock.Operations.cs
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Threading;

public sealed partial class PiQAsyncLock
{
    #region LoggerMessage Delegates
    private static readonly Action<ILogger, string, double, Exception?> _logTryAcquire =
        LoggerMessage.Define<string, double>(
            LogLevel.Debug,
            new EventId(6, "TryAcquire"),
            "Trying to acquire lock {LockId} with timeout {Timeout}ms");

    private static readonly Action<ILogger, string, Exception?> _logAcquiredWithTimeout =
        LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(7, "AcquiredWithTimeout"),
            "Successfully acquired lock {LockId} with timeout");

    private static readonly Action<ILogger, string, double, Exception?> _logAcquisitionTimeout =
        LoggerMessage.Define<string, double>(
            LogLevel.Debug,
            new EventId(8, "AcquisitionTimeout"),
            "Timeout acquiring lock {LockId} after {Timeout}ms");

    private static readonly Action<ILogger, string, Exception?> _logReleased =
        LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(9, "Released"),
            "Released lock {LockId}");

    private static readonly Action<ILogger, string, Exception?> _logAttemptedReleaseWhenNotHeld =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(10, "AttemptedReleaseWhenNotHeld"),
            "Attempted to release lock {LockId} when it wasn't held");

    private static readonly Action<ILogger, string, Exception> _logReleaseError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(11, "ReleaseError"),
            "Error occurred while releasing lock {LockId}");
    #endregion

    /// <summary>
    /// Tries to acquire the lock within the specified timeout
    /// </summary>
    /// <param name="timeout">Maximum time to wait for the lock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A disposable that releases the lock if acquired, otherwise null</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is negative</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the lock has been disposed</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled</exception>
    public async Task<IDisposable?> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero");
        }

        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_logger != null)
        {
            _logTryAcquire(_logger, LockId, timeout.TotalMilliseconds, null);
        }

        try
        {
            Interlocked.Increment(ref _pendingOperations);
            bool acquired = await _semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);

            if (acquired)
            {
                if (_logger != null)
                {
                    _logAcquiredWithTimeout(_logger, LockId, null);
                }
                return new AsyncLockReleaser(this);
            }
            else
            {
                Interlocked.Decrement(ref _pendingOperations);
                if (_logger != null)
                {
                    _logAcquisitionTimeout(_logger, LockId, timeout.TotalMilliseconds, null);
                }
                return null;
            }
        }
        catch (OperationCanceledException)
        {
            Interlocked.Decrement(ref _pendingOperations);
            if (_logger != null)
            {
                _logAcquisitionCancelled(_logger, LockId, null);
            }
            throw;
        }
        catch (Exception ex)
        {
            Interlocked.Decrement(ref _pendingOperations);
            if (_logger != null)
            {
                _logAcquisitionError(_logger, LockId, ex);
            }
            throw;
        }
    }

    /// <summary>
    /// Releases the lock
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (IsHeld)
            {
                _semaphore.Release();
                if (_logger != null)
                {
                    _logReleased(_logger, LockId, null);
                }
            }
            else if (_logger != null)
            {
                _logAttemptedReleaseWhenNotHeld(_logger, LockId, null);
            }
        }
        catch (Exception ex)
        {
            if (_logger != null)
            {
                _logReleaseError(_logger, LockId, ex);
            }
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Releases the lock
    /// </summary>
    internal void Release()
    {
        try
        {
            if (!_disposed && IsHeld)
            {
                _semaphore.Release();
                if (_logger != null)
                {
                    _logReleased(_logger, LockId, null);
                }
            }
        }
        catch (Exception ex) when (ex is not ObjectDisposedException)
        {
            if (_logger != null)
            {
                _logReleaseError(_logger, LockId, ex);
            }
        }
        finally
        {
            Interlocked.Decrement(ref _pendingOperations);
        }
    }
}