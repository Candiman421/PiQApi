// PiQApi.Abstractions/Core/ICertAsyncLock.cs
namespace PiQApi.Abstractions.Core;

/// <summary>
/// Interface for asynchronous lock that supports thread-safe access to resources
/// </summary>
public interface ICertAsyncLock : IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier for this lock
    /// </summary>
    string LockId { get; }

    /// <summary>
    /// Gets whether the lock is currently held
    /// </summary>
    bool IsHeld { get; }

    /// <summary>
    /// Acquires the lock asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A disposable that releases the lock when disposed</returns>
    Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to acquire the lock within the specified timeout
    /// </summary>
    /// <param name="timeout">Maximum time to wait for the lock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A disposable that releases the lock if acquired, otherwise null</returns>
    Task<IDisposable?> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases the lock
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}