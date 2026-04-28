// PiQApi.Abstractions/Core/Interfaces/IConfigurationManager.cs
namespace PiQApi.Abstractions.Core.Interfaces
{
    /// <summary>
    /// Manages configuration settings and initialization
    /// </summary>
    public interface IConfigurationManager
    {
        /// <summary>
        /// Initializes the configuration manager
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
        /// <exception cref="InvalidOperationException">Thrown when initialization fails</exception>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the current configuration
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
        /// <exception cref="ConfigurationException">Thrown when configuration validation fails</exception>
        Task ValidateConfigurationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the system is properly configured
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the system is configured; otherwise, false</returns>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
        Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets the configuration to default values
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested</exception>
        /// <exception cref="InvalidOperationException">Thrown when reset operation fails</exception>
        Task ResetConfigurationAsync(CancellationToken cancellationToken = default);
    }
}