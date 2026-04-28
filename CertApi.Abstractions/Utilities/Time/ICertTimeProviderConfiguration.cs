// CertApi.Abstractions/Utilities/Time/ICertTimeProviderConfiguration.cs
namespace CertApi.Abstractions.Utilities.Time;

/// <summary>
/// Interface for configuring time provider in non-DI contexts
/// </summary>
public interface ICertTimeProviderConfiguration
{
    /// <summary>
    /// Configures the time provider to use
    /// </summary>
    /// <param name="provider">The time provider implementation</param>
    void ConfigureTimeProvider(ICertTimeProvider provider);
}
