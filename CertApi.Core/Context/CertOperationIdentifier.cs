// CertApi.Core/Context/CertOperationIdentifier.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Enums;

namespace CertApi.Core.Context;

/// <summary>
/// Implementation of the operation identifier interface
/// </summary>
public class CertOperationIdentifier : ICertOperationIdentifier
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CertOperationIdentifier"/> class
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="name">Operation name</param>
    /// <param name="operationType">Operation type</param>
    /// <param name="parent">Parent operation identifier</param>
    /// <param name="startTime">Start time of the operation</param>
    /// <param name="timeout">Timeout for the operation</param>
    public CertOperationIdentifier(
        string id,
        string name,
        OperationType operationType,
        ICertOperationIdentifier? parent,
        DateTimeOffset startTime,
        TimeSpan timeout)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(name);

        Id = id;
        Name = name;
        OperationType = operationType;
        Parent = parent;
        StartTime = startTime;
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the unique identifier for this operation
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the name of the operation
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of operation
    /// </summary>
    public OperationType OperationType { get; }

    /// <summary>
    /// Gets the parent operation identifier, if any
    /// </summary>
    public ICertOperationIdentifier? Parent { get; }

    /// <summary>
    /// Gets the start time of the operation
    /// </summary>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the timeout for the operation
    /// </summary>
    public TimeSpan Timeout { get; }
}