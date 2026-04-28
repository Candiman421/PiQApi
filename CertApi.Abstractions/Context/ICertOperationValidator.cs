// CertApi.Abstractions/Context/ICertOperationValidator.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Results;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Abstractions.Context
{
    /// <summary>
    /// Interface for operation validator
    /// </summary>
    public interface ICertOperationValidator
    {
        /// <summary>
        /// Validates an entity asynchronously
        /// </summary>
        Task<ICertValidationResult> ValidateAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Validates an entity as required asynchronously
        /// </summary>
        Task<ICertValidationResult> ValidateRequiredAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Validates an entity and creates a result asynchronously
        /// </summary>
        Task<ICertResult<T>> ValidateAndCreateResultAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Creates a validation context
        /// </summary>
        ICertValidationContext CreateValidationContext(ValidationModeType mode = ValidationModeType.Standard, CancellationToken? cancellationToken = null);
    }
}