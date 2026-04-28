// CertApi.Core/Threading/CertAsyncLock.Releaser.cs
namespace CertApi.Core.Threading;

public sealed partial class CertAsyncLock
{
    private sealed class AsyncLockReleaser : IDisposable
    {
        private readonly CertAsyncLock _lock;
        private bool _disposed;

        public AsyncLockReleaser(CertAsyncLock @lock)
        {
            _lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _lock.Release();
        }
    }
}