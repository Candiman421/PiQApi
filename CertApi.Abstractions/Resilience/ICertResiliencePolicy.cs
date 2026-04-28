// CertApi.Abstractions/Resilience/ICertResiliencePolicy.cs
namespace CertApi.Abstractions.Resilience;

/// <summary>
/// Defines a policy for handling resilience in operations
/// </summary>
public interface ICertResiliencePolicy
{
    /// <summary>
    /// Executes an operation with the policy
    /// </summary>
    /// <typeparam name="T">Return type of the operation</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken);

    /// <summary>
    /// Executes an operation with the policy
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}