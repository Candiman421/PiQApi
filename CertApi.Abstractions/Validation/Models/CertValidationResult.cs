// CertApi.Abstractions/Validation/Models/CertValidationResult.cs
using System.Globalization;
using System.Text;
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Validation.Models;

/// <summary>
/// Base class for validation results
/// </summary>
public abstract class CertValidationResult : ICertValidationResult
{
    /// <summary>
    /// Gets whether the validation result is valid
    /// </summary>
    public abstract bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation errors
    /// </summary>
    public abstract IReadOnlyList<CertValidationError> Errors { get; }

    /// <summary>
    /// Gets the exception that caused validation to fail, if any
    /// </summary>
    public abstract Exception? Exception { get; }

    /// <summary>
    /// Gets additional context values for the validation result
    /// </summary>
    public abstract IReadOnlyDictionary<string, object> Context { get; }

    /// <summary>
    /// Determines if the validation result has any errors
    /// </summary>
    /// <returns>True if the result contains errors; otherwise, false</returns>
    public bool HasErrors()
    {
        return Errors.Count > 0;
    }

    /// <summary>
    /// Gets errors with a specific severity
    /// </summary>
    /// <param name="severity">The severity to filter by</param>
    /// <returns>Collection of errors with the specified severity</returns>
    public IEnumerable<CertValidationError> GetErrorsBySeverity(ValidationSeverityType severity)
    {
        return Errors.Where(e => e.Severity == severity);
    }

    /// <summary>
    /// Gets the correlation ID from the validation context
    /// </summary>
    /// <returns>The correlation ID, or null if not found</returns>
    public string? GetCorrelationId()
    {
        if (Context.TryGetValue("CorrelationId", out var correlationId) && correlationId is string id)
        {
            return id;
        }

        return null;
    }

    /// <summary>
    /// Gets a summary of all error messages in the validation result
    /// </summary>
    /// <returns>A summary of all error messages</returns>
    public string GetErrorSummary()
    {
        if (!HasErrors() || Errors.Count == 0)
        {
            return string.Empty;
        }

        if (Errors.Count == 1)
        {
            return Errors[0].Message;
        }

        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture, $"Multiple validation errors:");

        for (int i = 0; i < Errors.Count; i++)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $" - {Errors[i].Message}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Creates a valid validation result with no errors
    /// </summary>
    /// <returns>A valid validation result</returns>
    public static ICertValidationResult Valid()
    {
        return new CertValidationResultIsValid();
    }
}
