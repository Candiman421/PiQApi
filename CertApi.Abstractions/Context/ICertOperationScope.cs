// CertApi.Abstractions/Context/ICertOperationScope.cs
namespace CertApi.Abstractions.Context;

/// <summary>
/// Interface for operation scopes that track timing and metrics
/// </summary>
public interface ICertOperationScope : IDisposable
{
    /// <summary>
    /// Gets the name of the scope
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the parent operation context
    /// </summary>
    ICertOperationContext Context { get; }

    /// <summary>
    /// Gets whether the scope is active
    /// </summary>
    bool IsActive { get; }
}