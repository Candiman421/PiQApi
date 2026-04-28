// CertApi.Abstractions/Context/ICertOperationContextFactory.cs
using CertApi.Abstractions.Core;

namespace CertApi.Abstractions.Context;

/// <summary>
/// Factory for creating operation contexts
/// </summary>
public interface ICertOperationContextFactory
{
    /// <summary>
    /// Creates a context with the specified operation name and correlation context
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <returns>Operation context</returns>
    ICertOperationContext Create(string operationName, ICertCorrelationContext correlationContext);

    /// <summary>
    /// Creates a base operation context
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An initialized operation context</returns>
    Task<ICertOperationContext> CreateInitializedContextAsync(CancellationToken cancellationToken = default);
}
