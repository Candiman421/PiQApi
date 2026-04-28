// CertApi.Ews.Core/Interfaces/Context/IEwsOperationContextFactory.cs

using CertApi.Abstractions.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Ews.Core.Interfaces.Context
{
    /// <summary>
    /// Factory for creating Ews operation contexts
    /// </summary>
    public interface IEwsOperationContextFactory
    {
        /// <summary>
        /// Creates a basic Ews operation context
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A new Ews operation context</returns>
        Task<IEwsOperationContext> CreateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an Ews context with the specified properties
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="ewsCorrelationContext">Ews correlation context</param>
        /// <returns>Ews operation context</returns>
        IEwsOperationContext Create(string operationName, IEwsCorrelationContext ewsCorrelationContext);

        /// <summary>
        /// Creates a standalone Ews context with its own correlation context
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operationType">Type of operation</param>
        /// <param name="timeout">Operation timeout</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="userPrincipalName">Optional user principal name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Ews operation context</returns>
        IEwsOperationContext CreateStandalone(
            string operationName,
            OperationType operationType = OperationType.Generic,
            TimeSpan? timeout = null,
            string? tenantId = null,
            string? requestId = null,
            string? userPrincipalName = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates and initializes an Ews operation context
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operationType">Type of operation</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="userPrincipalName">Optional user principal name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Initialized Ews operation context</returns>
        Task<IEwsOperationContext> CreateInitializedContextAsync(
            string operationName,
            OperationType operationType = OperationType.Generic,
            string? tenantId = null,
            string? requestId = null,
            string? userPrincipalName = null,
            CancellationToken cancellationToken = default);
    }
}