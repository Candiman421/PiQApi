// PiQApi.Abstractions/Utilities/Time/IPiQTimeProviderConfiguration.cs
namespace PiQApi.Abstractions.Utilities.Time;

/// <summary>
/// Interface for configuring time provider in non-DI contexts
/// </summary>
public interface IPiQTimeProviderConfiguration
{
    /// <summary>
    /// Configures the time provider to use
    /// </summary>
    /// <param name="provider">The time provider implementation</param>
    void ConfigureTimeProvider(IPiQTimeProvider provider);
}
