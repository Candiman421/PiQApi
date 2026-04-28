// CertApi.Ews.Core/Interfaces/Context/IEwsOperationContext.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Validation;
using CertApi.Ews.Core.Interfaces.Context;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Ews.Core.Interfaces.Context
{
    /// <summary>
    /// Interface for Exchange operation context
    /// </summary>
    public interface IEwsOperationContext : ICertOperationContext
    {
        /// <summary>
        /// Gets the EWS-specific metrics
        /// </summary>
        new IEwsOperationMetrics Metrics { get; }

        /// <summary>
        /// Gets the correlation ID
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Gets the operation ID
        /// </summary>
        string OperationId { get; }

        /// <summary>
        /// Gets the correlation context
        /// </summary>
        new ICertCorrelationContext CorrelationContext { get; }

        /// <summary>
        /// Gets the validation context
        /// </summary>
        ICertValidationContext ValidationContext { get; }

        /// <summary>
        /// Gets the properties collection
        /// </summary>
        new ICertOperationProperties Properties { get; }

        /// <summary>
        /// Logs the start of an operation
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        new Task LogOperationStartAsync();

        /// <summary>
        /// Logs the end of an operation
        /// </summary>
        /// <param name="success">Whether the operation was successful</param>
        /// <returns>A task representing the asynchronous operation</returns>
        new Task LogOperationEndAsync(bool success);
    }
}
