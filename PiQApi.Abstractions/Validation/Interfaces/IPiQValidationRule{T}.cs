// PiQApi.Abstractions/Validation/Interfaces/IPiQValidationRule{T}.cs
namespace PiQApi.Abstractions.Validation.Interfaces
{
    /// <summary>
    /// Defines a validation rule for a specific entity type
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public interface IPiQValidationRule<T> : IPiQValidationRule where T : class
    {
        /// <summary>
        /// Validates the entity synchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        PiQValidationResult Validate(T entity, PiQValidationContext context);

        /// <summary>
        /// Validates the entity asynchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<PiQValidationResult> ValidateAsync(T entity, PiQValidationContext context, CancellationToken cancellationToken = default);
    }
}