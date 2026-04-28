// PiQApi.Core/Utilities/Disposables/CertDisposableAction.cs
namespace PiQApi.Core.Utilities.Disposables;

/// <summary>
/// A utility class that executes an action when disposed
/// </summary>
public class CertDisposableAction : IDisposable
{
    private readonly Action _action;
    private bool _disposed;

    /// <summary>
    /// Creates a new disposable action
    /// </summary>
    /// <param name="action">The action to execute when disposed</param>
    public CertDisposableAction(Action action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Executes the action and disposes the instance
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources and executes the action
    /// </summary>
    /// <param name="disposing">True if called from Dispose</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            _action();
        }

        _disposed = true;
    }
}
