// PiQApi.Abstractions/Context/IPiQOperationContext.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Abstractions.Context;

/// <summary>
/// Provides comprehensive contextual information for service operations.
/// Uses composition to include operation-specific information.
/// </summary>
public interface IPiQOperationContext : IPiQOperationProperties, IPiQOperationLifecycle, IPiQOperationLogger, IPiQOperationValidator, IPiQOperationResources
{
    /// <summary>
    /// Gets the operation identifier information
    /// </summary>
    IPiQOperationIdentifier Identifier { get; }

    /// <summary>
    /// Gets the operation state information
    /// </summary>
    IPiQOperationState State { get; }

    /// <summary>
    /// Gets the operation metrics information
    /// </summary>
    IPiQOperationMetrics Metrics { get; }

    /// <summary>
    /// Gets the underlying correlation context
    /// </summary>
    IPiQCorrelationContext CorrelationContext { get; }
}
