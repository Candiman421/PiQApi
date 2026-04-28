// CertApi.Abstractions/Utilities/Time/ICertTimeProviderFactory.cs
namespace CertApi.Abstractions.Utilities.Time;

/// <summary>
/// Factory for accessing time provider instances
/// </summary>
public interface ICertTimeProviderFactory
{
    /// <summary>
    /// Gets the current ICertTimeProvider instance
    /// </summary>
    ICertTimeProvider Current { get; }

    /// <summary>
    /// Configures the factory with a specific ICertTimeProvider implementation
    /// </summary>
    /// <param name="provider">The time provider to use</param>
    void Configure(ICertTimeProvider provider);

    /// <summary>
    /// Resets the time provider to the default implementation
    /// </summary>
    void Reset();
}