// CertApi.Core/Validation/CertCoreValidationResult.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Validation.Models;

namespace CertApi.Core.Validation;

/// <summary>
/// Implementation of validation result
/// </summary>
public class CertCoreValidationResult : CertValidationResult
{
    private readonly List<CertValidationError> _errors;

    /// <summary>
    /// Gets whether the validation result is valid
    /// </summary>
    public override bool IsValid => !Errors.Any(e => e.Severity == ValidationSeverityType.Error);

    /// <summary>
    /// Gets the collection of validation errors
    /// </summary>
    public override IReadOnlyList<CertValidationError> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Gets the exception that caused validation to fail, if any
    /// </summary>
    public override Exception? Exception { get; }

    /// <summary>
    /// Gets additional context values for the validation result
    /// </summary>
    public override IReadOnlyDictionary<string, object> Context { get; }

    /// <summary>
    /// Initializes a new instance of the CertCoreValidationResult class
    /// </summary>
    /// <param name="errors">Collection of validation errors</param>
    /// <param name="exception">Exception that caused validation to fail, if any</param>
    /// <param name="context">Additional context values</param>
    public CertCoreValidationResult(
        IEnumerable<CertValidationError>? errors = null,
        Exception? exception = null,
        IDictionary<string, object>? context = null)
    {
        _errors = errors?.ToList() ?? new List<CertValidationError>();
        Exception = exception;
        Context = context?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Adds an error to the validation result
    /// </summary>
    /// <param name="error">The validation error to add</param>
    /// <returns>The current instance to support method chaining</returns>
    public CertCoreValidationResult AddError(CertValidationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _errors.Add(error);
        return this;
    }

    /// <summary>
    /// Adds an error to the validation result
    /// </summary>
    /// <param name="propertyName">The name of the property with the error</param>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code</param>
    /// <param name="severity">The error severity</param>
    /// <returns>The current instance to support method chaining</returns>
    public CertCoreValidationResult AddError(
        string propertyName,
        string message,
        string code = "ValidationError",
        ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        var error = new CertValidationError(propertyName, message, code, severity);
        return AddError(error);
    }

    /// <summary>
    /// Merges errors from another validation result
    /// </summary>
    /// <param name="other">The validation result to merge errors from</param>
    /// <returns>The current instance to support method chaining</returns>
    public CertCoreValidationResult MergeErrors(CertValidationResult other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var error in other.Errors)
        {
            _errors.Add(error);
        }

        return this;
    }
}
