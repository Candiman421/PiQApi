// CertApi.Abstractions/Core/ICertAsyncLockFactory.cs
namespace CertApi.Abstractions.Core;

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
