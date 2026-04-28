// PiQApi.Core/Exceptions/Infrastructure/PiQResourceLockedException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when attempting to access a locked resource
/// </summary>
public sealed class PiQResourceLockedException : PiQServiceException
{
    /// <summary>
    /// Gets the identifier of the locked resource
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Gets the identifier of the lock owner
    /// </summary>
    public string LockOwner { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceLockedException"/> class
    /// </summary>
    public PiQResourceLockedException()
        : base("Resource is locked", "ResourceLocked")
    {
        ResourceId = string.Empty;
        LockOwner = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceLockedException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PiQResourceLockedException(string message)
        : base(message, "ResourceLocked")
    {
        ResourceId = string.Empty;
        LockOwner = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceLockedException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQResourceLockedException(string message, Exception? inner)
        : base(message, "ResourceLocked", inner)
    {
        ResourceId = string.Empty;
        LockOwner = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceLockedException"/> class with a specified resource identifier
    /// and lock owner
    /// </summary>
    /// <param name="resourceId">The identifier of the locked resource</param>
    /// <param name="lockOwner">The identifier of the lock owner</param>
    public PiQResourceLockedException(string resourceId, string? lockOwner)
        : base($"Resource {resourceId} is locked by {lockOwner ?? "unknown"}", "ResourceLocked")
    {
        ResourceId = resourceId ?? string.Empty;
        LockOwner = lockOwner ?? string.Empty;

        if (!string.IsNullOrEmpty(resourceId))
            AddData(nameof(ResourceId), resourceId);

        if (!string.IsNullOrEmpty(lockOwner))
            AddData(nameof(LockOwner), lockOwner);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQResourceLockedException"/> class with a specified resource identifier,
    /// lock owner, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="resourceId">The identifier of the locked resource</param>
    /// <param name="lockOwner">The identifier of the lock owner</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQResourceLockedException(string resourceId, string? lockOwner, Exception? inner)
        : base($"Resource {resourceId} is locked by {lockOwner ?? "unknown"}", "ResourceLocked", inner)
    {
        ResourceId = resourceId ?? string.Empty;
        LockOwner = lockOwner ?? string.Empty;

        if (!string.IsNullOrEmpty(resourceId))
            AddData(nameof(ResourceId), resourceId);

        if (!string.IsNullOrEmpty(lockOwner))
            AddData(nameof(LockOwner), lockOwner);
    }
}