// PiQApi.Core/Threading/PiQAsyncLock.Releaser.cs
namespace PiQApi.Core.Threading;

public sealed partial class PiQAsyncLock
{
    private sealed class AsyncLockReleaser : IDisposable
    {
        private readonly PiQAsyncLock _lock;
        private bool _disposed;

        public AsyncLockReleaser(PiQAsyncLock @lock)
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