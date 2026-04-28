// PiQApi.Abstractions/Enums/ConflictResolutionModeType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines conflict resolution modes for service operations
/// </summary>
public enum ConflictResolutionModeType
{
    /// <summary>
    /// Automatically resolves conflicts
    /// </summary>
    AutoResolve = 0,

    /// <summary>
    /// Always overwrites conflicting items
    /// </summary>
    AlwaysOverwrite = 1,

    /// <summary>
    /// Never overwrites conflicting items
    /// </summary>
    NeverOverwrite = 2
}