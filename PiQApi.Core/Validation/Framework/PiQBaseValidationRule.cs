// PiQApi.Core/Validation/Framework/PiQBaseValidationRule.cs
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
    public abstract class PiQBaseValidationRule<T> : PiQValidationRule<T> where T : class
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the PiQBaseValidationRule class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="resultFactory">The validation result factory</param>
        protected PiQBaseValidationRule(ILogger logger, IPiQValidationResultFactory resultFactory)
            : base(logger, resultFactory)
        {
        }

        #endregion

        #region Base Validation Methods

        /// <summary>
        /// Validates the entity
        /// </summary>
        public override IPiQValidationResult Validate(T entity, IPiQValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            // Simplified implementation always returns success
            return ResultFactory.Success();
        }

        /// <summary>
        /// Validates the entity asynchronously
        /// </summary>
        public override async Task<IPiQValidationResult> ValidateAsync(T entity, IPiQValidationContext context, CancellationToken cancellationToken = default)
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
        protected PiQValidationError CreateError(string propertyName, string message, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            return new PiQValidationError(propertyName, message, errorCode, severity);
        }

        /// <summary>
        /// Internal validation method - in simplified version, returns success
        /// </summary>
        protected override IPiQValidationResult ValidateInternal(T entity, IPiQValidationContext context)
        {
            return ResultFactory.Success();
        }

        /// <summary>
        /// Internal async validation method - default implementation calls sync version
        /// </summary>
        protected virtual Task<IPiQValidationResult> ValidateInternalAsync(
            T entity,
            IPiQValidationContext context,
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
    public abstract class PiQSyncValidationRule<T> : PiQBaseValidationRule<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the PiQSyncValidationRule class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="resultFactory">The validation result factory</param>
        protected PiQSyncValidationRule(ILogger logger, IPiQValidationResultFactory resultFactory)
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
    public abstract class PiQAsyncValidationRule<T> : PiQBaseValidationRule<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the PiQAsyncValidationRule class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="resultFactory">The validation result factory</param>
        protected PiQAsyncValidationRule(ILogger logger, IPiQValidationResultFactory resultFactory)
            : base(logger, resultFactory)
        {
        }
    }

    #endregion
}