// PiQApi.Abstractions/Context/IPiQOperationContextFactory.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Abstractions.Context;

/// <summary>
/// Factory for creating operation contexts
/// </summary>
public interface IPiQOperationContextFactory
{
    /// <summary>
    /// Creates a context with the specified operation name and correlation context
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <returns>Operation context</returns>
    IPiQOperationContext Create(string operationName, IPiQCorrelationContext correlationContext);

    /// <summary>
    /// Creates a base operation context
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An initialized operation context</returns>
    Task<IPiQOperationContext> CreateInitializedContextAsync(CancellationToken cancellationToken = default);
}
