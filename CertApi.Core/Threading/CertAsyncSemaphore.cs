// CertApi.Core/Threading/CertAsyncSemaphore.cs
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Threading;

/// <summary>
/// Provides an asynchronous semaphore implementation that supports cancellation
/// </summary>
public sealed class CertAsyncSemaphore : IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<CertAsyncSemaphore>? _logger;
    private readonly string _id;
    private int _pendingOperations;
    private bool _disposed;

    #region LoggerMessage Delegates
    private static readonly Action<ILogger, string, Exception?> _logWaiting =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, "Waiting"),
            "Waiting for semaphore {SemaphoreId}");

    private static readonly Action<ILogger, string, Exception?> _logAcquired =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, "Acquired"),
            "Acquired semaphore {SemaphoreId}");

    private static readonly Action<ILogger, string, Exception?> _logReleased =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(3, "Released"),
            "Released semaphore {SemaphoreId}");

    private static readonly Action<ILogger, string, int, Exception?> _logDisposing =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(4, "Disposing"),
            "Disposing semaphore {SemaphoreId} with {PendingOperations} pending operations");
    #endregion

    /// <summary>
    /// Gets the identifier for this semaphore
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// Gets the current count of the semaphore
    /// </summary>
    public int CurrentCount => _semaphore.CurrentCount;

    /// <summary>
    /// Initializes a new instance of the CertAsyncSemaphore class
    /// </summary>
    /// <param name="initialCount">The initial number of requests that can be granted concurrently</param>
    /// <param name="maxCount">The maximum number of requests that can be granted concurrently</param>
    public CertAsyncSemaphore(int initialCount, int maxCount)
        : this(Guid.NewGuid().ToString(), initialCount, maxCount, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the CertAsyncSemaphore class with an ID
    /// </summary>
    /// <param name="id">The identifier for this semaphore</param>
    /// <param name="initialCount">The initial number of requests that can be granted concurrently</param>
    /// <param name="maxCount">The maximum number of requests that can be granted concurrently</param>
    /// <param name="logger">Optional logger instance</param>
    public CertAsyncSemaphore(string id, int initialCount, int maxCount, ILogger<CertAsyncSemaphore>? logger = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        if (initialCount < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCount), "Initial count cannot be negative");
        if (maxCount < initialCount)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max count cannot be less than initial count");

        _id = id;
        _semaphore = new SemaphoreSlim(initialCount, maxCount);
        _logger = logger;
    }

    /// <summary>
    /// Waits to enter the semaphore asynchronously
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A disposable that releases the semaphore when disposed</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the semaphore has been disposed</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Interlocked.Increment(ref _pendingOperations);
        if (_logger != null)
        {
            _logWaiting(_logger, _id, null);
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_logger != null)
            {
                _logAcquired(_logger, _id, null);
            }

            return new SemaphoreReleaser(this);
        }
        catch
        {
            Interlocked.Decrement(ref _pendingOperations);
            throw;
        }
    }

    /// <summary>
    /// Attempts to enter the semaphore asynchronously within the specified timeout
    /// </summary>
    /// <param name="timeout">The timeout</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A disposable that releases the semaphore if entry was successful; otherwise, null</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the semaphore has been disposed</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    public async Task<bool> TryWaitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive");

        ObjectDisposedException.ThrowIf(_disposed, this);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            // Try to acquire the semaphore
            await _semaphore.WaitAsync(cts.Token).ConfigureAwait(false);

            // Release it if successful
            _semaphore.Release();

            return true;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // This was our timeout, not an external cancellation
            return false;
        }
    }

    /// <summary>
    /// Releases the semaphore
    /// </summary>
    internal void Release()
    {
        try
        {
            if (!_disposed)
            {
                _semaphore.Release();
                if (_logger != null)
                {
                    _logReleased(_logger, _id, null);
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore if already disposed
        }
        finally
        {
            Interlocked.Decrement(ref _pendingOperations);
        }
    }

    /// <summary>
    /// Disposes the semaphore
    /// </summary>
    public ValueTask DisposeAsync()
    {
        if (_disposed)
            return default;

        _disposed = true;
        if (_logger != null)
        {
            _logDisposing(_logger, _id, _pendingOperations, null);
        }
        _semaphore.Dispose();

        return default;
    }

    private sealed class SemaphoreReleaser : IDisposable
    {
        private readonly CertAsyncSemaphore _semaphore;
        private bool _disposed;

        public SemaphoreReleaser(CertAsyncSemaphore semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _semaphore.Release();
        }
    }
}