// PiQApi.Abstractions/Core/IPiQAsyncLockFactory.cs
namespace PiQApi.Abstractions.Core;

/// <summary>
/// Factory for creating asynchronous locks
/// </summary>
public interface IPiQAsyncLockFactory
{
    /// <summary>
    /// Creates a new asynchronous lock
    /// </summary>
    /// <returns>Asynchronous lock</returns>
    IPiQAsyncLock Create();
}
