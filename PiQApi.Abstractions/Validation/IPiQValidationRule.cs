// PiQApi.Abstractions/Validation/IPiQValidationRule.cs
using PiQApi.Abstractions.Validation.Models;

namespace PiQApi.Abstractions.Validation;

/// <summary>
/// Interface for a validation rule
/// </summary>
public interface IPiQValidationRule
{
    /// <summary>
    /// Gets the identifier of the rule
    /// </summary>
    string RuleId { get; }
}

/// <summary>
/// Interface for a validation rule for a specific type
/// </summary>
/// <typeparam name="T">The type of entity to validate</typeparam>
public interface IPiQValidationRule<T> : IPiQValidationRule where T : class
{
    /// <summary>
    /// Validates the entity
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <param name="context">The validation context</param>
    /// <returns>The validation result</returns>
    IPiQValidationResult Validate(T entity, IPiQValidationContext context);

    /// <summary>
    /// Validates the entity asynchronously
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <param name="context">The validation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The validation result</returns>
    Task<IPiQValidationResult> ValidateAsync(T entity, IPiQValidationContext context, CancellationToken cancellationToken = default);
}
