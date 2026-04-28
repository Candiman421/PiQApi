// PiQApi.Abstractions/Context/IPiQOperationIdentifier.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Context;

/// <summary>
/// Provides identification information for operations
/// </summary>
public interface IPiQOperationIdentifier
{
    /// <summary>
    /// Gets the unique identifier for this operation
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the name of the operation
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of operation
    /// </summary>
    OperationType OperationType { get; }

    /// <summary>
    /// Gets the parent operation identifier, if any
    /// </summary>
    IPiQOperationIdentifier? Parent { get; }

    /// <summary>
    /// Gets the start time of the operation
    /// </summary>
    DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the timeout for the operation
    /// </summary>
    TimeSpan Timeout { get; }
}