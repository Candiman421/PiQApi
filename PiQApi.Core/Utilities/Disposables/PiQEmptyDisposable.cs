// PiQApi.Core/Utilities/Disposables/PiQEmptyDisposable.cs
namespace PiQApi.Core.Utilities.Disposables;

/// <summary>
/// Empty implementation of IDisposable that does nothing when disposed.
/// Used as a fallback when other sources might return null.
/// </summary>
public sealed class PiQEmptyDisposable : IDisposable
{
    private PiQEmptyDisposable() { }

    /// <summary>
    /// Does nothing when disposed.
    /// </summary>
    public void Dispose() { }

    /// <summary>
    /// Singleton instance to avoid creating multiple instances.
    /// </summary>
    public static IDisposable Instance { get; } = new PiQEmptyDisposable();
}
