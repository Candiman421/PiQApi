// CertApi.Core/Validation/CertValidationRuleResourceBuilder.cs

namespace CertApi.Core.Validation;

/// <summary>
/// Builder for creating validation rule resources
/// </summary>
public class CertValidationRuleResourceBuilder
{
    private string? _resourceId;
    private string? _resourceType;
    private string? _name;
    private bool _isModified;
    private bool _isDeleted;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _modifiedAt;
    private Dictionary<string, object>? _properties;

    /// <summary>
    /// Sets the resource ID
    /// </summary>
    /// <param name="resourceId">The resource ID</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithResourceId(string resourceId)
    {
        _resourceId = resourceId;
        return this;
    }

    /// <summary>
    /// Sets the resource type
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithResourceType(string resourceType)
    {
        _resourceType = resourceType;
        return this;
    }

    /// <summary>
    /// Sets the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Marks the resource as modified
    /// </summary>
    /// <param name="isModified">Whether the resource is modified</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithIsModified(bool isModified = true)
    {
        _isModified = isModified;
        if (isModified && !_modifiedAt.HasValue)
        {
            _modifiedAt = DateTimeOffset.UtcNow;
        }
        return this;
    }

    /// <summary>
    /// Marks the resource as deleted
    /// </summary>
    /// <param name="isDeleted">Whether the resource is deleted</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithIsDeleted(bool isDeleted = true)
    {
        _isDeleted = isDeleted;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp
    /// </summary>
    /// <param name="createdAt">The creation timestamp</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the modified timestamp
    /// </summary>
    /// <param name="modifiedAt">The modified timestamp</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithModifiedAt(DateTimeOffset? modifiedAt)
    {
        _modifiedAt = modifiedAt;
        return this;
    }

    /// <summary>
    /// Sets the properties
    /// </summary>
    /// <param name="properties">The properties</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder WithProperties(Dictionary<string, object> properties)
    {
        _properties = properties;
        return this;
    }

    /// <summary>
    /// Adds a property
    /// </summary>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value</param>
    /// <returns>This builder for method chaining</returns>
    public CertValidationRuleResourceBuilder AddProperty(string key, object value)
    {
        _properties ??= new Dictionary<string, object>();
        _properties[key] = value;
        return this;
    }

    /// <summary>
    /// Builds a new validation rule resource
    /// </summary>
    /// <returns>The built validation rule resource</returns>
    public CertValidationRuleResource Build()
    {
        if (string.IsNullOrEmpty(_resourceId))
            throw new InvalidOperationException("ResourceId must be specified");

        if (string.IsNullOrEmpty(_resourceType))
            throw new InvalidOperationException("ResourceType must be specified");

        return new CertValidationRuleResource(
            _resourceId,
            _resourceType,
            _name ?? _resourceId,
            _isModified,
            _isDeleted,
            _createdAt,
            _modifiedAt,
            _properties
        );
    }
}