// PiQApi.Abstractions/Resilience/IPiQResiliencePolicyExecutor.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Resilience;

/// <summary>
/// Interface for executing operations with resilience policies
/// </summary>
public interface IPiQResiliencePolicyExecutor
{
    /// <summary>
    /// Executes an operation with the specified policy type
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation with the specified policy type that returns no result
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteAsync(
        Func<Task> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);
}
