// CertApi.Abstractions/Core/ICertCorrelationAware.cs
namespace CertApi.Abstractions.Core;

/// <summary>
/// Interface for components that are aware of correlation context
/// </summary>
public interface ICertCorrelationAware
{
    /// <summary>
    /// Gets the correlation context
    /// </summary>
    ICertCorrelationContext CorrelationContext { get; }
}