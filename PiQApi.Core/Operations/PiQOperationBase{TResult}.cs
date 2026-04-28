// PiQApi.Core/Operations/PiQOperationBase{TResult}.cs

using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Operations;
using PiQApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Operations;

/// <summary>
/// Base implementation for operations that return a result
/// </summary>
/// <typeparam name="TResult">The type of result this operation returns</typeparam>
public abstract class PiQOperationBase<TResult> : PiQOperationBase, IPiQOperation<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PiQOperationBase{TResult}"/> class
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="asyncLock">Asynchronous lock for thread-safety</param>
    /// <param name="logger">Logger</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    protected PiQOperationBase(
        IPiQOperationContext context,
        IPiQAsyncLock asyncLock,
        ILogger logger,
        IPiQTimeProvider? timeProvider = null)
        : base(context, asyncLock, logger, timeProvider)
    {
    }

    /// <summary>
    /// Executes the operation and returns the result
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The operation result</returns>
    public abstract Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the operation with proper context tracking, validation, and error handling
    /// </summary>
    /// <param name="operation">The operation to execute</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    protected async Task<TResult> ExecuteWithTrackingAsync(
        Func<CancellationToken, Task<TResult>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        // Ensure the operation is valid
        if (!IsReady)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        await ValidateStateAsync(cancellationToken).ConfigureAwait(false);

        // Execute the operation with full tracking
        return await ExecuteOperationAsync(operation, operationName, cancellationToken).ConfigureAwait(false);
    }
}