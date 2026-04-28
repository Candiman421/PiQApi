// PiQApi.Ews.Operations/Core/Interfaces/IEwsOperationBase.cs
using PiQApi.Abstractions.Operations;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Service.Core.Interfaces;

namespace PiQApi.Ews.Operations.Core.Interfaces
{
    /// <summary>
    /// Base interface for all Exchange operations
    /// </summary>
    public interface IEwsOperationBase : ICertOperationBase, IAsyncDisposable
    {
        /// <summary>
        /// Gets the operation ID
        /// </summary>
        string OperationId { get; }

        /// <summary>
        /// Gets the correlation ID
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Gets whether the operation is ready
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Gets the Exchange operation context
        /// </summary>
        IEwsOperationContext Context { get; }

        /// <summary>
        /// Gets the Exchange service wrapper
        /// </summary>
        IExchangeServiceWrapper ServiceWrapper { get; }

        /// <summary>
        /// Validates the operation state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ValidateStateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the operation is operational
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the operation is operational; otherwise, false</returns>
        Task<bool> IsOperationalAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans up resources used by the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CleanupAsync(CancellationToken cancellationToken = default);
    }
}