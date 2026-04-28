// PiQApi.Abstractions/Core/Interfaces/IAsyncLockFactory.cs
namespace PiQApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Factory for creating asynchronous locks
    /// </summary>
    public interface IAsyncLockFactory
    {
        /// <summary>
        /// Creates a new asynchronous lock
        /// </summary>
        /// <returns>Asynchronous lock</returns>
        IAsyncLock Create();
    }
}