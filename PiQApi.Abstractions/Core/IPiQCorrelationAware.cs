// PiQApi.Abstractions/Core/IPiQCorrelationAware.cs
namespace PiQApi.Abstractions.Core;

/// <summary>
/// Interface for components that are aware of correlation context
/// </summary>
public interface IPiQCorrelationAware
{
    /// <summary>
    /// Gets the correlation context
    /// </summary>
    IPiQCorrelationContext CorrelationContext { get; }
}