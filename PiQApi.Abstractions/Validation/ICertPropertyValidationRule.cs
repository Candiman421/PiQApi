// PiQApi.Abstractions/Validation/ICertPropertyValidationRule.cs
namespace PiQApi.Abstractions.Validation;

/// <summary>
/// Interface for property validation rules
/// </summary>
/// <typeparam name="T">The type of entity to validate</typeparam>
public interface ICertPropertyValidationRule<T> : ICertValidationRule<T> where T : class
{
    /// <summary>
    /// Gets the property name
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Validates the property
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>True if validation succeeds; otherwise, false</returns>
    bool ValidateProperty(T entity, out string? errorMessage);

    /// <summary>
    /// Validates the property asynchronously
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing validation result and error message</returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidatePropertyAsync(T entity, CancellationToken cancellationToken = default);
}