// PiQApi.Abstractions/Context/IPiQOperationScope.cs
namespace PiQApi.Abstractions.Context;

/// <summary>
/// Interface for operation scopes that track timing and metrics
/// </summary>
public interface IPiQOperationScope : IDisposable
{
    /// <summary>
    /// Gets the name of the scope
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the parent operation context
    /// </summary>
    IPiQOperationContext Context { get; }

    /// <summary>
    /// Gets whether the scope is active
    /// </summary>
    bool IsActive { get; }
}