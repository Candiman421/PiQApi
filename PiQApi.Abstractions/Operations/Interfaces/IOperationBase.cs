// PiQApi.Abstractions/Operations/Interfaces/IOperationBase.cs
using PiQApi.Abstractions.Context;

namespace PiQApi.Abstractions.Operations.Interfaces
{
    /// <summary>
    /// Defines the base interface for all operation implementations
    /// </summary>
    public interface IOperationBase : IAsyncDisposable
    {
        /// <summary>
        /// Gets the unique identifier for the operation
        /// </summary>
        string OperationId { get; }

        /// <summary>
        /// Gets the current correlation identifier for the operation
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Gets whether the operation is currently active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Indicates if the operation is ready for use
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Current operation context
        /// </summary>
        IOperationContext Context { get; }

        /// <summary>
        /// Initializes the operation
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the operation's current state
        /// </summary>
        Task ValidateStateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the operation can be executed
        /// </summary>
        Task<bool> IsOperationalAsync(CancellationToken cancellationToken = default);
    }
}