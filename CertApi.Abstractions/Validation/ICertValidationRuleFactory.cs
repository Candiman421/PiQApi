// CertApi.Abstractions/Validation/ICertValidationRuleFactory.cs
namespace CertApi.Abstractions.Validation;

/// <summary>
/// Factory for creating validation rule instances
/// </summary>
public interface ICertValidationRuleFactory
{
    /// <summary>
    /// Creates a validation rule instance of the specified type
    /// </summary>
    /// <typeparam name="T">Type of the validation rule</typeparam>
    /// <returns>Instance of the validation rule</returns>
    T CreateRule<T>() where T : ICertValidationRule;

    /// <summary>
    /// Creates validation rules for a specific entity type
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <returns>Collection of validation rules</returns>
    IEnumerable<ICertValidationRule<TEntity>> GetRulesForEntity<TEntity>() where TEntity : class;
}
