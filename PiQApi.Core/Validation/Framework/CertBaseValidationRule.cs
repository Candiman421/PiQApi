// PiQApi.Core/Validation/Framework/CertBaseValidationRule.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Core.Validation.Framework
{
    #region Base Validation Rule

    /// <summary>
    /// Base implementation of validation rule with validation method hooks (simplified)
    /// </summary>
    /// <typeparam name="T">The type of entity to validate</typeparam>
    public abstract class CertBaseValidationRule<T> : CertValidationRule<T> where T : class
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the CertBaseValidationRule class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="resultFactory">The validation result factory</param>
        protected CertBaseValidationRule(ILogger logger, ICertValidationResultFactory resultFactory)
            : base(logger, resultFactory)
        {
        }

        #endregion

        #region Base Validation Methods

        /// <summary>
        /// Validates the entity
        /// </summary>
        public override ICertValidationResult Validate(T entity, ICertValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            // Simplified implementation always returns success
            return ResultFactory.Success();
        }

        /// <summary>
        /// Validates the entity asynchronously
        /// </summary>
        public override async Task<ICertValidationResult> ValidateAsync(T entity, ICertValidationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            // Simplified implementation always returns success
            return await Task.FromResult(ResultFactory.Success()).ConfigureAwait(false);
        }

        #endregion

        #region Protected Helper Methods

        /// <summary>
        /// Creates a validation error (helper method kept for compatibility)
        /// </summary>
        protected CertValidationError CreateError(string propertyName, string message, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            return new CertValidationError(propertyName, message, errorCode, severity);
        }

        /// <summary>
        /// Internal validation method - in simplified version, returns success
        /// </summary>
        protected override ICertValidationResult ValidateInternal(T entity, ICertValidationContext context)
        {
            return ResultFactory.Success();
        }

        /// <summary>
        /// Internal async validation method - default implementation calls sync version
        /// </summary>
        protected virtual Task<ICertValidationResult> ValidateInternalAsync(
            T entity,
            ICertValidationContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResultFactory.Success());
        }

        #endregion
    }

    #endregion

    #region Synchronous Validation Rule (Simplified)

    /// <summary>
    /// Base class for synchronous validation rules (simplified)
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public abstract class CertSyncValidationRule<T> : CertBaseValidationRule<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the CertSyncValidationRule class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="resultFactory">The validation result factory</param>
        protected CertSyncValidationRule(ILogger logger, ICertValidationResultFactory resultFactory)
            : base(logger, resultFactory)
        {
        }
    }

    #endregion

    #region Asynchronous Validation Rule (Simplified)

    /// <summary>
    /// Base class for asynchronous validation rules (simplified)
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public abstract class CertAsyncValidationRule<T> : CertBaseValidationRule<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the CertAsyncValidationRule class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="resultFactory">The validation result factory</param>
        protected CertAsyncValidationRule(ILogger logger, ICertValidationResultFactory resultFactory)
            : base(logger, resultFactory)
        {
        }
    }

    #endregion
}