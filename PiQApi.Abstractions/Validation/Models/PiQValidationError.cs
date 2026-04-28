// PiQApi.Abstractions/Validation/Models/PiQValidationError.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Validation.Models;

/// <summary>
/// Represents a validation error
/// </summary>
public class PiQValidationError
{
    /// <summary>
    /// Gets the error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the error message
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the severity of the error
    /// </summary>
    public ValidationSeverityType Severity { get; }

    /// <summary>
    /// Gets the property path if this error is related to a specific property
    /// </summary>
    public string? PropertyPath { get; }

    /// <summary>
    /// Gets the property name (shorthand for the last segment of PropertyPath)
    /// </summary>
    public string PropertyName => PropertyPath?.Contains('.', StringComparison.Ordinal) == true
        ? PropertyPath[(PropertyPath.LastIndexOf('.') + 1)..]
        : PropertyPath ?? string.Empty;

    /// <summary>
    /// Gets the error code
    /// </summary>
    public string Code => ErrorCode;

    /// <summary>
    /// Gets the error message
    /// </summary>
    public string Message => ErrorMessage;

    /// <summary>
    /// Creates a new validation error
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="severity">Error severity</param>
    public PiQValidationError(
        string propertyName,
        string errorMessage,
        string errorCode = "ValidationError",
        ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        PropertyPath = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Severity = severity;
    }

    /// <summary>
    /// Returns a string representation of the error
    /// </summary>
    public override string ToString()
    {
        return PropertyPath != null
            ? $"{ErrorCode} ({Severity}): {ErrorMessage} [Property: {PropertyPath}]"
            : $"{ErrorCode} ({Severity}): {ErrorMessage}";
    }
}