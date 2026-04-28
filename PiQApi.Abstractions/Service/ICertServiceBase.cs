// PiQApi.Abstractions/Service/ICertServiceBase.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Service;

/// <summary>
/// Base interface for all service implementations
/// </summary>
public interface ICertServiceBase
{
    /// <summary>
    /// Gets the current service instance
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service instance</returns>
    Task<object> GetServiceAsync(ICertOperationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation with the service
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="context">Operation context</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Policy type to apply</param>
    /// <param name="operationName">Name of the operation for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteAsync<T>(
        ICertOperationContext context,
        Func<object, Task<T>> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation with the service without a return value
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyType">Policy type to apply</param>
    /// <param name="operationName">Name of the operation for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteAsync(
        ICertOperationContext context,
        Func<object, Task> operation,
        ResiliencePolicyType policyType,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the service is authenticated
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task EnsureAuthenticatedAsync(ICertOperationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the correlation ID for the service
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    void SetCorrelationId(string correlationId);
}