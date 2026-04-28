// CertApi.Abstractions/Context/ICertOperationContext.cs
using CertApi.Abstractions.Core;

namespace CertApi.Abstractions.Context;

/// <summary>
/// Provides comprehensive contextual information for service operations.
/// Uses composition to include operation-specific information.
/// </summary>
public interface ICertOperationContext : ICertOperationProperties, ICertOperationLifecycle, ICertOperationLogger, ICertOperationValidator, ICertOperationResources
{
    /// <summary>
    /// Gets the operation identifier information
    /// </summary>
    ICertOperationIdentifier Identifier { get; }

    /// <summary>
    /// Gets the operation state information
    /// </summary>
    ICertOperationState State { get; }

    /// <summary>
    /// Gets the operation metrics information
    /// </summary>
    ICertOperationMetrics Metrics { get; }

    /// <summary>
    /// Gets the underlying correlation context
    /// </summary>
    ICertCorrelationContext CorrelationContext { get; }
}
