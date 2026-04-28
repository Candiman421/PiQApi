// PiQApi.Abstractions/Resilience/IPiQServiceResiliencePolicyExecutor.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;

namespace PiQApi.Abstractions.Resilience;

/// <summary>
/// Interface for executing service layer operations with resilience policies
/// </summary>
public interface IPiQServiceResiliencePolicyExecutor : IPiQResiliencePolicyExecutor
{
    /// <summary>
    /// Executes a service operation with the specified policy type
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteServiceAsync<T>(
        Func<Task<T>> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a service operation with the specified policy type that returns no result
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteServiceAsync(
        Func<Task> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a service operation with mapping to a service result
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="exceptionMapper">Function to map exceptions to service results</param>
    /// <param name="policyType">Type of policy to apply</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service result of the operation</returns>
    Task<IPiQServiceResult<T>> ExecuteWithServiceMappingAsync<T>(
        Func<Task<IPiQServiceResult<T>>> operation,
        Func<Exception, IPiQOperationContext, IPiQServiceResult<T>> exceptionMapper,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);
}