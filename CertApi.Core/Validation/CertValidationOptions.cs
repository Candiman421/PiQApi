// CertApi.Core/Validation/CertValidationOptions.cs
using System.ComponentModel.DataAnnotations;

namespace CertApi.Core.Validation;

/// <summary>
/// Configuration options for generic validation
/// </summary>
public record CertValidationOptions
{
    /// <summary>
    /// Gets or sets whether required properties validation is enabled
    /// </summary>
    public bool RequiredPropertiesValidation { get; init; } = true;

    /// <summary>
    /// Gets or sets whether data annotations validation is enabled
    /// </summary>
    public bool ValidateDataAnnotations { get; init; } = true;

    /// <summary>
    /// Gets or sets the validation scope
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    /// Gets or sets whether to fail fast on validation errors
    /// </summary>
    public bool FailFast { get; init; }

    /// <summary>
    /// Gets or sets the maximum validation depth
    /// </summary>
    [Range(1, 100)]
    public int MaxDepth { get; init; } = 5;
}