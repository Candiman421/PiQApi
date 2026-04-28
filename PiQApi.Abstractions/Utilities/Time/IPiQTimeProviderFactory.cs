// PiQApi.Abstractions/Utilities/Time/IPiQTimeProviderFactory.cs
namespace PiQApi.Abstractions.Utilities.Time;

/// <summary>
/// Factory for accessing time provider instances
/// </summary>
public interface IPiQTimeProviderFactory
{
    /// <summary>
    /// Gets the current IPiQTimeProvider instance
    /// </summary>
    IPiQTimeProvider Current { get; }

    /// <summary>
    /// Configures the factory with a specific IPiQTimeProvider implementation
    /// </summary>
    /// <param name="provider">The time provider to use</param>
    void Configure(IPiQTimeProvider provider);

    /// <summary>
    /// Resets the time provider to the default implementation
    /// </summary>
    void Reset();
}