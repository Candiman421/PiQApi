// PiQApi.Ews.Core/Interfaces/IEwsServiceBase.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Service;
using PiQApi.Ews.Core.Interfaces.Context;
using Microsoft.Exchange.WebServices.Data;
using Task = System.Threading.Tasks.Task;

namespace PiQApi.Ews.Core.Interfaces
{
    /// <summary>
    /// Base interface for all Exchange Web Services operations
    /// Extends the core service base interface with EWS-specific functionality
    /// </summary>
    public interface IEwsServiceBase : ICertServiceBase
    {
        /// <summary>
        /// Gets the current Exchange service instance
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Exchange service instance</returns>
        Task<ExchangeService> GetServiceAsync(IEwsOperationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an operation with the Exchange service
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="context">EWS operation context</param>
        /// <param name="operation">Operation to execute</param>
        /// <param name="policyType">Policy type to apply</param>
        /// <param name="operationName">Name of the operation for tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        Task<T> ExecuteAsync<T>(
            IEwsOperationContext context,
            Func<ExchangeService, Task<T>> operation,
            ResiliencePolicyType policyType,
            string operationName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an operation with the Exchange service without a return value
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="operation">Operation to execute</param>
        /// <param name="policyType">Policy type to apply</param>
        /// <param name="operationName">Name of the operation for tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExecuteAsync(
            IEwsOperationContext context,
            Func<ExchangeService, Task> operation,
            ResiliencePolicyType policyType,
            string operationName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ensures the service is authenticated
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task EnsureAuthenticatedAsync(IEwsOperationContext context, CancellationToken cancellationToken = default);
    }
}