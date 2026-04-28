// PiQApi.Abstractions/Validation/Models/CertValidationResultIsValid.cs
namespace PiQApi.Abstractions.Validation.Models;

/// <summary>
/// Implementation of a valid validation result with no errors
/// </summary>
public class CertValidationResultIsValid : CertValidationResult
{
    private static readonly IReadOnlyList<CertValidationError> _emptyErrors = Array.Empty<CertValidationError>();
    private static readonly IReadOnlyDictionary<string, object> _emptyContext = new Dictionary<string, object>(StringComparer.Ordinal).AsReadOnly();

    /// <summary>
    /// Gets whether the validation result is valid, always true for this implementation
    /// </summary>
    public override bool IsValid => true;

    /// <summary>
    /// Gets the collection of validation errors, always empty for this implementation
    /// </summary>
    public override IReadOnlyList<CertValidationError> Errors => _emptyErrors;

    /// <summary>
    /// Gets the exception that caused validation to fail, always null for this implementation
    /// </summary>
    public override Exception? Exception => null;

    /// <summary>
    /// Gets additional context values for the validation result, always empty for this implementation
    /// </summary>
    public override IReadOnlyDictionary<string, object> Context => _emptyContext;
}
