// CertApi.Abstractions/Validation/ICertValidationRule.cs
using CertApi.Abstractions.Validation.Models;

namespace CertApi.Abstractions.Validation;

/// <summary>
/// Interface for a validation rule
/// </summary>
public interface ICertValidationRule
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
public interface ICertValidationRule<T> : ICertValidationRule where T : class
{
    /// <summary>
    /// Validates the entity
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <param name="context">The validation context</param>
    /// <returns>The validation result</returns>
    ICertValidationResult Validate(T entity, ICertValidationContext context);

    /// <summary>
    /// Validates the entity asynchronously
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <param name="context">The validation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The validation result</returns>
    Task<ICertValidationResult> ValidateAsync(T entity, ICertValidationContext context, CancellationToken cancellationToken = default);
}
