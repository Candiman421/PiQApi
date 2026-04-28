// CertApi.Core/Core/CertCorrelationScopeState.cs
namespace CertApi.Core.Core;

/// <summary>
/// Represents the state of a correlation scope
/// </summary>
internal sealed class CertCorrelationScopeState
{
    /// <summary>
    /// Gets the parent ID for this scope
    /// </summary>
    public string? ParentId { get; }

    /// <summary>
    /// Creates a new correlation scope state
    /// </summary>
    /// <param name="parentId">The parent ID</param>
    public CertCorrelationScopeState(string? parentId)
    {
        ParentId = parentId;
    }
}