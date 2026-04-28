// PiQApi.Ews.Core/Interfaces/Context/IEwsOperationContext.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Validation;
using PiQApi.Ews.Core.Interfaces.Context;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Ews.Core.Interfaces.Context
{
    /// <summary>
    /// Interface for Exchange operation context
    /// </summary>
    public interface IEwsOperationContext : IPiQOperationContext
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
        new IPiQCorrelationContext CorrelationContext { get; }

        /// <summary>
        /// Gets the validation context
        /// </summary>
        IPiQValidationContext ValidationContext { get; }

        /// <summary>
        /// Gets the properties collection
        /// </summary>
        new IPiQOperationProperties Properties { get; }

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
