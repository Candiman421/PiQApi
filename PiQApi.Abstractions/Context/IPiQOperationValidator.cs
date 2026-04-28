// PiQApi.Abstractions/Context/IPiQOperationValidator.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Abstractions.Context
{
    /// <summary>
    /// Interface for operation validator
    /// </summary>
    public interface IPiQOperationValidator
    {
        /// <summary>
        /// Validates an entity asynchronously
        /// </summary>
        Task<IPiQValidationResult> ValidateAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Validates an entity as required asynchronously
        /// </summary>
        Task<IPiQValidationResult> ValidateRequiredAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Validates an entity and creates a result asynchronously
        /// </summary>
        Task<IPiQResult<T>> ValidateAndCreateResultAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Creates a validation context
        /// </summary>
        IPiQValidationContext CreateValidationContext(ValidationModeType mode = ValidationModeType.Standard, CancellationToken? cancellationToken = null);
    }
}