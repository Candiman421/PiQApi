// PiQApi.Abstractions/Core/ICertAsyncLockFactory.cs
namespace PiQApi.Abstractions.Core;

/// <summary>
/// Factory for creating asynchronous locks
/// </summary>
public interface ICertAsyncLockFactory
{
    /// <summary>
    /// Creates a new asynchronous lock
    /// </summary>
    /// <returns>Asynchronous lock</returns>
    ICertAsyncLock Create();
}
