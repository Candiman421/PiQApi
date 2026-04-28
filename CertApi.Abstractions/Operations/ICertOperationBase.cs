// CertApi.Abstractions/Operations/ICertOperationBase.cs

namespace CertApi.Abstractions.Operations
{
    /// <summary>
    /// Base interface for all operations in the system
    /// </summary>
    public interface ICertOperationBase
    {
        /// <summary>
        /// Initializes the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans up any resources used by the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CleanupAsync(CancellationToken cancellationToken = default);
    }
}
