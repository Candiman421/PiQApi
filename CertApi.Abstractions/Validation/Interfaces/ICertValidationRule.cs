// CertApi.Abstractions/Validation/Interfaces/ICertValidationRule.cs
namespace CertApi.Abstractions.Validation.Interfaces
{
    /// <summary>
    /// Base interface for all validation rules
    /// </summary>
    public interface ICertValidationRule
    {
        /// <summary>
        /// Gets the unique identifier for this validation rule
        /// </summary>
        string RuleId { get; }
    }
}