// CertApi.Core/Exceptions/Infrastructure/CertResourceNotFoundException.cs
using CertApi.Core.Exceptions.Base;

namespace CertApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when a requested resource cannot be found
/// </summary>
public sealed class CertResourceNotFoundException : CertServiceException
{
    /// <summary>
    /// Gets the type of the resource that was not found
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Gets the identifier of the resource that was not found
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceNotFoundException"/> class
    /// </summary>
    public CertResourceNotFoundException()
        : base("Resource not found", "ResourceNotFound")
    {
        ResourceType = string.Empty;
        ResourceId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceNotFoundException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public CertResourceNotFoundException(string message)
        : base(message, "ResourceNotFound")
    {
        ResourceType = string.Empty;
        ResourceId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceNotFoundException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertResourceNotFoundException(string message, Exception? inner)
        : base(message, "ResourceNotFound", inner)
    {
        ResourceType = string.Empty;
        ResourceId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceNotFoundException"/> class with a specified resource type
    /// and resource identifier
    /// </summary>
    /// <param name="resourceType">The type of the resource that was not found</param>
    /// <param name="resourceId">The identifier of the resource that was not found</param>
    public CertResourceNotFoundException(string resourceType, string resourceId)
        : base($"Resource of type {resourceType} with ID {resourceId} not found", "ResourceNotFound")
    {
        ResourceType = resourceType ?? string.Empty;
        ResourceId = resourceId ?? string.Empty;

        if (!string.IsNullOrEmpty(resourceType))
            AddData(nameof(ResourceType), resourceType);

        if (!string.IsNullOrEmpty(resourceId))
            AddData(nameof(ResourceId), resourceId);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResourceNotFoundException"/> class with a specified resource type,
    /// resource identifier, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="resourceType">The type of the resource that was not found</param>
    /// <param name="resourceId">The identifier of the resource that was not found</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertResourceNotFoundException(string resourceType, string resourceId, Exception? inner)
        : base($"Resource of type {resourceType} with ID {resourceId} not found", "ResourceNotFound", inner)
    {
        ResourceType = resourceType ?? string.Empty;
        ResourceId = resourceId ?? string.Empty;

        if (!string.IsNullOrEmpty(resourceType))
            AddData(nameof(ResourceType), resourceType);

        if (!string.IsNullOrEmpty(resourceId))
            AddData(nameof(ResourceId), resourceId);
    }

    /// <summary>
    /// Creates a new instance for folder resource not found
    /// </summary>
    /// <param name="folderId">The identifier of the folder</param>
    /// <param name="folderPath">The path of the folder</param>
    /// <param name="inner">The inner exception</param>
    /// <returns>A new <see cref="CertResourceNotFoundException"/> for folder resources</returns>
    public static CertResourceNotFoundException ForFolder(string folderId, string folderPath = "", Exception? inner = null)
    {
        var exception = new CertResourceNotFoundException("Folder", folderId, inner);

        if (!string.IsNullOrEmpty(folderPath))
            exception.AddData("FolderPath", folderPath);

        return exception;
    }

    /// <summary>
    /// Creates a new instance for item resource not found
    /// </summary>
    /// <param name="itemId">The identifier of the item</param>
    /// <param name="itemType">The type of the item</param>
    /// <param name="inner">The inner exception</param>
    /// <returns>A new <see cref="CertResourceNotFoundException"/> for item resources</returns>
    public static CertResourceNotFoundException ForItem(string itemId, string? itemType = null, Exception? inner = null)
    {
        var exception = new CertResourceNotFoundException(itemType ?? "Item", itemId, inner);
        return exception;
    }
}