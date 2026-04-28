// PiQApi.Abstractions/Context/IPiQCorrelationScope.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Abstractions.Context;

/// <summary>
/// Interface for correlation scopes that track distributed operations
/// </summary>
public interface IPiQCorrelationScope : IDisposable
{
    /// <summary>
    /// Gets the correlation ID for the scope
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the parent correlation context
    /// </summary>
    IPiQCorrelationContext Context { get; }

    /// <summary>
    /// Gets whether the scope is active
    /// </summary>
    bool IsActive { get; }
}
