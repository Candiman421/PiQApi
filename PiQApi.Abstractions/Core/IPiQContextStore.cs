// PiQApi.Abstractions/Core/IPiQContextStore.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Abstractions.Core;

/// <summary>
/// Interface for storing and retrieving the current correlation context
/// Created to make testing easier by allowing injection of test implementations
/// </summary>
public interface IPiQContextStore
{
    /// <summary>
    /// Gets the current correlation context
    /// </summary>
    /// <returns>The current correlation context or null if not set</returns>
    IPiQCorrelationContext? GetCurrentContext();

    /// <summary>
    /// Sets the current correlation context
    /// </summary>
    /// <param name="context">The correlation context to set as current</param>
    void SetCurrentContext(IPiQCorrelationContext context);
}