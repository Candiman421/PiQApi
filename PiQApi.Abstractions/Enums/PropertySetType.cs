// PiQApi.Abstractions/Enums/PropertySetType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines the type of property set to use when retrieving items
/// </summary>
public enum PropertySetType
{
    /// <summary>
    /// Only returns item/folder IDs
    /// </summary>
    IdOnly = 0,

    /// <summary>
    /// Returns first class properties (common properties like subject, from, to, etc.)
    /// </summary>
    FirstClass = 1,

    /// <summary>
    /// Returns all available properties for the item
    /// </summary>
    AllProperties = 2,

    /// <summary>
    /// Returns only binary properties (attachments, MIME content)
    /// </summary>
    BinaryProperties = 3,

    /// <summary>
    /// Returns only MIME content
    /// </summary>
    MimeContent = 4,

    /// <summary>
    /// Returns a custom set of properties defined by the caller
    /// </summary>
    Custom = 5
}