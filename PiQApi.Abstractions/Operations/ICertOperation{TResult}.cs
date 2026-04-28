// PiQApi.Abstractions/Operations/ICertOperationBase.cs

namespace PiQApi.Abstractions.Operations
{
    /// <summary>
    /// Interface for operations that return a result
    /// </summary>
    /// <typeparam name="TResult">The type of result this operation returns</typeparam>
    public interface ICertOperation<TResult> : ICertOperationBase
    {
        /// <summary>
        /// Executes the operation and returns the result
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The operation result</returns>
        Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
