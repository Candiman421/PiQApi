// CertApi.Ews.Service/Core/Interfaces/IEwsPolicyExecutor.cs
using CertApi.Ews.Core.Enums;

namespace CertApi.Ews.Service.Core.Interfaces
{
    /// <summary>
    /// Interface for executing operations with resilience policies
    /// </summary>
    public interface IEwsPolicyExecutor
    {
        /// <summary>
        /// Executes an operation with the specified policy type
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">Operation to execute</param>
        /// <param name="ewsPolicyType">Type of policy to apply</param>
        /// <param name="operationName">Name of the operation for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation,
            EwsPolicyType ewsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an operation with the specified policy type without a return value
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="ewsPolicyType">Type of policy to apply</param>
        /// <param name="operationName">Name of the operation for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExecuteAsync(
            Func<Task> operation,
            EwsPolicyType ewsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default);
    }
}
