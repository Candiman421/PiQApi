// CertApi.Abstractions/Resilience/ICertOperationResiliencePolicyExecutor.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Results;

namespace CertApi.Abstractions.Resilience;

/// <summary>
/// Interface for executing business operation layer operations with resilience policies
/// </summary>
public interface ICertOperationResiliencePolicyExecutor : ICertResiliencePolicyExecutor
{
    /// <summary>
    /// Executes a business operation with the specified policy type
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteBusinessOperationAsync<T>(
        Func<Task<T>> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a business operation with the specified policy type that returns no result
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteBusinessOperationAsync(
        Func<Task> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a business operation with mapping to a business operation result
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="exceptionMapper">Function to map exceptions to operation results</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<ICertResult<T>> ExecuteWithOperationMappingAsync<T>(
        Func<Task<ICertResult<T>>> operation,
        Func<Exception, ICertOperationContext, ICertResult<T>> exceptionMapper,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);
}