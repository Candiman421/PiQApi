// PiQApi.Abstractions/Validation/Interfaces/IPiQValidationRule.cs
namespace PiQApi.Abstractions.Validation.Interfaces
{
    /// <summary>
    /// Base interface for all validation rules
    /// </summary>
    public interface IPiQValidationRule
    {
        /// <summary>
        /// Gets the unique identifier for this validation rule
        /// </summary>
        string RuleId { get; }
    }
}