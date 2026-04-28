// PiQApi.Core/Utilities/Disposables/CertEmptyDisposable.cs
namespace PiQApi.Core.Utilities.Disposables;

/// <summary>
/// Empty implementation of IDisposable that does nothing when disposed.
/// Used as a fallback when other sources might return null.
/// </summary>
public sealed class CertEmptyDisposable : IDisposable
{
    private CertEmptyDisposable() { }

    /// <summary>
    /// Does nothing when disposed.
    /// </summary>
    public void Dispose() { }

    /// <summary>
    /// Singleton instance to avoid creating multiple instances.
    /// </summary>
    public static IDisposable Instance { get; } = new CertEmptyDisposable();
}
