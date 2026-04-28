// CertApi.Core/Context/CertOperationValidator.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Results;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using CertApi.Core.Results;
using CertApi.Core.Validation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Core.Context
{
    /// <summary>
    /// Implementation of validation functionality for operation contexts
    /// </summary>
    public class CertOperationValidator : ICertOperationValidator
    {
        private readonly ICertValidationService _validationService;
        private ICertCorrelationContext _correlationContext; // Removed readonly to allow reassignment

        /// <summary>
        /// Initializes a new instance of the <see cref="CertOperationValidator"/> class
        /// </summary>
        /// <param name="validationService">Validation service</param>
        /// <param name="correlationContext">Correlation context</param>
        public CertOperationValidator(
            ICertValidationService validationService,
            ICertCorrelationContext correlationContext)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertOperationValidator"/> class
        /// </summary>
        /// <param name="validationService">Validation service</param>
        public CertOperationValidator(ICertValidationService validationService)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _correlationContext = null!; // Will be provided later
        }

        /// <summary>
        /// Sets the correlation context
        /// </summary>
        /// <param name="correlationContext">Correlation context</param>
        public void SetCorrelationContext(ICertCorrelationContext correlationContext)
        {
            ArgumentNullException.ThrowIfNull(correlationContext);

            if (_correlationContext == null)
            {
                _correlationContext = correlationContext;
            }
        }

        /// <summary>
        /// Validates an entity asynchronously
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public async Task<ICertValidationResult> ValidateAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            var context = CreateValidationContext(mode, cancellationToken);
            return await _validationService.ValidateAsync(entity, context, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates an entity as required asynchronously
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public Task<ICertValidationResult> ValidateRequiredAsync<T>(T entity, CancellationToken cancellationToken) where T : class
        {
            return ValidateAsync(entity, ValidationModeType.Required, cancellationToken);
        }

        /// <summary>
        /// Validates an entity and creates a result asynchronously
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result with the entity or validation errors</returns>
        public async Task<ICertResult<T>> ValidateAndCreateResultAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            var validationResult = await ValidateAsync(entity, mode, cancellationToken).ConfigureAwait(false);
            string correlationId = _correlationContext?.CorrelationId ?? Guid.NewGuid().ToString("N");

            // Create appropriate result based on validation
            if (validationResult.IsValid)
            {
                return CertResult<T>.CreateSuccess(entity, correlationId);
            }
            else
            {
                // Extract the first error if available
                var errors = validationResult.Errors;
                if (errors.Count > 0)
                {
                    var firstError = errors[0];
                    return CertResult<T>.CreateFailure(
                        firstError.Code, 
                        firstError.Message, 
                        correlationId);
                }

                // Handle case with no specific errors
                return CertResult<T>.CreateFailure(
                    "ValidationFailed",
                    "Validation failed with no specific errors",
                    correlationId);
            }
        }

        /// <summary>
        /// Creates a validation context
        /// </summary>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context</returns>
        public ICertValidationContext CreateValidationContext(ValidationModeType mode = ValidationModeType.Standard, CancellationToken? cancellationToken = null)
        {
            string correlationId = _correlationContext?.CorrelationId ?? Guid.NewGuid().ToString("N");
            var token = cancellationToken ?? CancellationToken.None;

            return _validationService.CreateContext(mode, token);
        }
    }
}