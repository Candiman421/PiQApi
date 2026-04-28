// CertApi.Abstractions/Core/Interfaces/IAsyncLock.cs
namespace CertApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Represents an asynchronous lock for controlling concurrent access to resources
    /// </summary>
    public interface IAsyncLock : IAsyncDisposable
    {
        /// <summary>
        /// Acquires the lock asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A disposable object that releases the lock when disposed</returns>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
        /// <exception cref="TimeoutException">Thrown when the lock cannot be acquired within the default timeout</exception>
        Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to acquire the lock asynchronously within the specified timeout
        /// </summary>
        /// <param name="timeout">Maximum time to wait for lock acquisition</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the lock was acquired; otherwise, false</returns>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is negative</exception>
        Task<bool> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases the lock asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="InvalidOperationException">Thrown when the lock is not held by the caller</exception>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
        Task ReleaseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets whether the lock is currently held
        /// </summary>
        bool IsHeld { get; }

        /// <summary>
        /// Gets the unique identifier for this lock
        /// </summary>
        string LockId { get; }
    }
}