// PiQApi.Core/Validation/CertValidationRuleResource.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;

namespace PiQApi.Core.Validation;

/// <summary>
/// Resource class for tracking validation rule executions
/// </summary>
public class CertValidationRuleResource : ICertResource
{
    /// <summary>
    /// Gets the resource identifier
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Gets the resource type
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Gets the resource name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets whether the resource is modified
    /// </summary>
    public bool IsModified { get; }

    /// <summary>
    /// Gets whether the resource is deleted
    /// </summary>
    public bool IsDeleted { get; }

    /// <summary>
    /// Gets when the resource was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets when the resource was last modified
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; }

    /// <summary>
    /// Gets the resource properties
    /// </summary>
    public Dictionary<string, object>? Properties { get; }

    /// <summary>
    /// Gets the error code if validation failed
    /// </summary>
    public ErrorCodeType? ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the CertValidationRuleResource class
    /// </summary>
    public CertValidationRuleResource(
        string resourceId,
        string resourceType,
        string name,
        bool isModified = false,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? modifiedAt = null,
        Dictionary<string, object>? properties = null,
        ErrorCodeType? errorCode = null)
    {
        ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IsModified = isModified;
        IsDeleted = isDeleted;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        ModifiedAt = modifiedAt;
        Properties = properties;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful validation resource
    /// </summary>
    public static CertValidationRuleResource Success(
        string resourceId,
        string resourceType,
        string name,
        Dictionary<string, object>? properties = null)
    {
        return new CertValidationRuleResource(
            resourceId,
            resourceType,
            name,
            properties: properties);
    }

    /// <summary>
    /// Creates a failed validation resource
    /// </summary>
    public static CertValidationRuleResource Failure(
        string resourceId,
        string resourceType,
        string name,
        ErrorCodeType errorCode,
        Dictionary<string, object>? properties = null)
    {
        properties ??= new Dictionary<string, object>();
        properties["ErrorCode"] = errorCode.ToString();

        return new CertValidationRuleResource(
            resourceId,
            resourceType,
            name,
            properties: properties,
            errorCode: errorCode);
    }
}