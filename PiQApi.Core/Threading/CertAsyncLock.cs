// PiQApi.Core/Threading/CertAsyncLock.cs
using PiQApi.Abstractions.Core;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Threading;

/// <summary>
/// Provides an asynchronous lock implementation that supports cancellation
/// </summary>
public sealed partial class CertAsyncLock : ICertAsyncLock, IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ILogger<CertAsyncLock>? _logger;
    private int _pendingOperations;
    private bool _disposed;

    #region LoggerMessage Delegates
    private static readonly Action<ILogger, string, int, Exception?> _logAcquiring =
        LoggerMessage.Define<string, int>(
            LogLevel.Debug,
            new EventId(1, "Acquiring"),
            "Acquiring lock {LockId}. Pending operations: {PendingOperations}");

    private static readonly Action<ILogger, string, Exception?> _logAcquired =
        LoggerMessage.Define<string>(
            LogLevel.Trace,
            new EventId(2, "Acquired"),
            "Acquired lock {LockId}");

    private static readonly Action<ILogger, string, Exception?> _logAcquisitionCancelled =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(3, "AcquisitionCancelled"),
            "Lock acquisition cancelled for {LockId}");

    private static readonly Action<ILogger, string, Exception> _logAcquisitionError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(4, "AcquisitionError"),
            "Error acquiring lock {LockId}");

    private static readonly Action<ILogger, string, int, Exception?> _logDisposing =
        LoggerMessage.Define<string, int>(
            LogLevel.Trace,
            new EventId(5, "Disposing"),
            "Disposing lock {LockId}. Pending operations: {PendingOperations}");

    // Logger delegates shared with CertAsyncLock.Operations.cs moved there to avoid duplication
    #endregion

    /// <summary>
    /// Gets whether the lock is currently held
    /// </summary>
    public bool IsHeld => _semaphore.CurrentCount == 0;

    /// <summary>
    /// Gets the unique identifier for this lock
    /// </summary>
    public string LockId { get; }

    /// <summary>
    /// Initializes a new instance of the CertAsyncLock class
    /// </summary>
    public CertAsyncLock() : this(Guid.NewGuid().ToString(), null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the CertAsyncLock class
    /// </summary>
    /// <param name="lockId">The unique identifier for this lock</param>
    /// <param name="logger">Logger for lock operations</param>
    public CertAsyncLock(
        string lockId,
        ILogger<CertAsyncLock>? logger = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(lockId);
        LockId = lockId;
        _logger = logger;
    }

    /// <summary>
    /// Acquires the lock asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A disposable that releases the lock when disposed</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the lock has been disposed</exception>
    /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Interlocked.Increment(ref _pendingOperations);
        if (_logger != null)
        {
            _logAcquiring(_logger, LockId, _pendingOperations, null);
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_logger != null)
            {
                _logAcquired(_logger, LockId, null);
            }
            return new AsyncLockReleaser(this);
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
    /// Disposes resources used by the lock
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_logger != null)
        {
            _logDisposing(_logger, LockId, _pendingOperations, null);
        }

        // Allow currently held lock to complete normally
        _semaphore.Dispose();

        await Task.CompletedTask.ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
