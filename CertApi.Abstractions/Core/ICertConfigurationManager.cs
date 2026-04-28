// CertApi.Abstractions/Core/ICertConfigurationManager.cs
namespace CertApi.Abstractions.Core;


//todo: A more comprehensive solution would be to create an ICertException hierarchy in the Abstractions project, but that would require more extensive changes to the codebase.

/// <summary>
/// Manages configuration settings and initialization
/// </summary>
public interface ICertConfigurationManager
{
    /// <summary>
    /// Initializes the configuration manager
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="System.OperationCanceledException">Thrown when cancellation is requested</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when initialization fails</exception>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the current configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="System.OperationCanceledException">Thrown when cancellation is requested</exception>
    /// <exception>Thrown when configuration validation fails</exception>
    Task ValidateConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the system is properly configured
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the system is configured; otherwise, false</returns>
    /// <exception cref="System.OperationCanceledException">Thrown when cancellation is requested</exception>
    Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the configuration to default values
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="System.OperationCanceledException">Thrown when cancellation is requested</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when reset operation fails</exception>
    Task ResetConfigurationAsync(CancellationToken cancellationToken = default);
}
