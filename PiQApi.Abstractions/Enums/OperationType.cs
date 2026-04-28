// PiQApi.Abstractions/Enums/OperationType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Defines the types of operations that can be performed within the service
/// </summary>
public enum OperationType
{
    /// <summary>
    /// Unspecified operation type (default)
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Generic operation type for general purpose operations
    /// </summary>
    Generic = 1,

    /// <summary>
    /// Read-only operation that retrieves data without modifications
    /// </summary>
    Read = 2,

    /// <summary>
    /// Write operation that creates new resources
    /// </summary>
    Create = 3,

    /// <summary>
    /// Write operation that updates existing resources
    /// </summary>
    Update = 4,

    /// <summary>
    /// Write operation that removes resources
    /// </summary>
    Delete = 5,

    /// <summary>
    /// Operation that performs a search or query
    /// </summary>
    Query = 6,

    /// <summary>
    /// Batch operation that processes multiple items
    /// </summary>
    Batch = 7,

    /// <summary>
    /// Operation that synchronizes data between systems
    /// </summary>
    Sync = 8,

    /// <summary>
    /// Administrative operation that affects system configuration
    /// </summary>
    Admin = 9,

    /// <summary>
    /// Authentication related operation
    /// </summary>
    Authentication = 10,

    /// <summary>
    /// Authorization related operation
    /// </summary>
    Authorization = 11,

    /// <summary>
    /// Operation that exports or downloads data
    /// </summary>
    Export = 12,

    /// <summary>
    /// Operation that imports or uploads data
    /// </summary>
    Import = 13,

    /// <summary>
    /// System maintenance operation
    /// </summary>
    Maintenance = 14,

    /// <summary>
    /// Operation that handles message processing
    /// </summary>
    MessageProcessing = 15,

    /// <summary>
    /// Calendar or scheduling related operation
    /// </summary>
    Calendar = 16,

    /// <summary>
    /// Contacts management operation
    /// </summary>
    Contacts = 17,

    /// <summary>
    /// Task management operation
    /// </summary>
    Tasks = 18
}