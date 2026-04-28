// CertApi.Abstractions/Validation/ICertValidationRuleBase.cs
namespace CertApi.Abstractions.Validation;

/// <summary>
/// Base interface for all validation rules
/// </summary>
public interface ICertValidationRuleBase
{
    /// <summary>
    /// Gets the unique identifier for this validation rule
    /// </summary>
    string RuleId { get; }
}