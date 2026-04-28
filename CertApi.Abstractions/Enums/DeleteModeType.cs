// CertApi.Abstractions/Enums/DeleteModeType.cs
namespace CertApi.Abstractions.Enums;

/// <summary>
/// Defines delete modes for service operations
/// </summary>
public enum DeleteModeType
{
    /// <summary>
    /// Permanently deletes items
    /// </summary>
    HardDelete = 0,

    /// <summary>
    /// Moves items to the Deleted Items folder
    /// </summary>
    MoveToDeletedItems = 1,

    /// <summary>
    /// Marks items as deleted but does not remove them
    /// </summary>
    SoftDelete = 2
}