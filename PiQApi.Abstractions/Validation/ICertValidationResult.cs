// PiQApi.Abstractions/Validation/ICertValidationResult.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Validation.Models;

namespace PiQApi.Abstractions.Validation;

/// <summary>
/// Interface for validation results
/// </summary>
public interface ICertValidationResult
{
    /// <summary>
    /// Gets whether the validation result is valid
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation errors
    /// </summary>
    IReadOnlyList<CertValidationError> Errors { get; }

    /// <summary>
    /// Gets the exception that caused validation to fail, if any
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Gets additional context values for the validation result
    /// </summary>
    IReadOnlyDictionary<string, object> Context { get; }

    /// <summary>
    /// Determines if the validation result has any errors
    /// </summary>
    /// <returns>True if the result contains errors; otherwise, false</returns>
    bool HasErrors();

    /// <summary>
    /// Gets errors with a specific severity
    /// </summary>
    /// <param name="severity">The severity to filter by</param>
    /// <returns>Collection of errors with the specified severity</returns>
    IEnumerable<CertValidationError> GetErrorsBySeverity(ValidationSeverityType severity);

    /// <summary>
    /// Gets the correlation ID from the validation context
    /// </summary>
    /// <returns>The correlation ID, or null if not found</returns>
    string? GetCorrelationId();

    /// <summary>
    /// Gets a summary of all error messages in the validation result
    /// </summary>
    /// <returns>A summary of all error messages</returns>
    string GetErrorSummary();
}
