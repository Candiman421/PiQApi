// PiQApi.Abstractions/Validation/Models/IPiQAsyncValidationRule.cs
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Abstractions.Validation.Models
{
    /// <summary>
    /// Interface for asynchronous validation rules
    /// </summary>
    /// <typeparam name="T">Type to validate</typeparam>
    public interface IPiQAsyncValidationRule<T> where T : class
    {
        /// <summary>
        /// Validates an entity asynchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<IPiQValidationResult> ValidateAsync(T entity, IPiQValidationContext context, CancellationToken cancellationToken = default);
    }
}