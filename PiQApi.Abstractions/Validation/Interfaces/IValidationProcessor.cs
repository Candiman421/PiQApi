// PiQApi.Abstractions/Validation/Interfaces/IValidationProcessor.cs
namespace PiQApi.Abstractions.Validation.Interfaces
{
    /// <summary>
    /// Defines a processor for validation rules
    /// </summary>
    public interface IValidationProcessor
    {
        /// <summary>
        /// Registers a collection of validation rules for the specified entity type
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="rules">Validation rules</param>
        void RegisterRules<T>(IEnumerable<ICertValidationRule<T>> rules) where T : class;

        /// <summary>
        /// Validates an entity using registered rules
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        CertValidationResult Validate<T>(T entity, CertValidationContext context) where T : class;

        /// <summary>
        /// Validates an entity asynchronously using registered rules
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<CertValidationResult> ValidateAsync<T>(T entity, CertValidationContext context, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Clears all registered validation rules
        /// </summary>
        void ClearRules();

        /// <summary>
        /// Gets the count of registered rules for the specified entity type
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>Count of rules</returns>
        int GetRuleCount<T>() where T : class;
    }
}