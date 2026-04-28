// PiQApi.Abstractions/Validation/Interfaces/ICertValidationRule{T}.cs
namespace PiQApi.Abstractions.Validation.Interfaces
{
    /// <summary>
    /// Defines a validation rule for a specific entity type
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public interface ICertValidationRule<T> : ICertValidationRule where T : class
    {
        /// <summary>
        /// Validates the entity synchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        CertValidationResult Validate(T entity, CertValidationContext context);

        /// <summary>
        /// Validates the entity asynchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<CertValidationResult> ValidateAsync(T entity, CertValidationContext context, CancellationToken cancellationToken = default);
    }
}