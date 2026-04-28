// CertApi.Abstractions/Core/ICertContextStore.cs
using CertApi.Abstractions.Core;

namespace CertApi.Abstractions.Core;

/// <summary>
/// Interface for storing and retrieving the current correlation context
/// Created to make testing easier by allowing injection of test implementations
/// </summary>
public interface ICertContextStore
{
    /// <summary>
    /// Gets the current correlation context
    /// </summary>
    /// <returns>The current correlation context or null if not set</returns>
    ICertCorrelationContext? GetCurrentContext();

    /// <summary>
    /// Sets the current correlation context
    /// </summary>
    /// <param name="context">The correlation context to set as current</param>
    void SetCurrentContext(ICertCorrelationContext context);
}