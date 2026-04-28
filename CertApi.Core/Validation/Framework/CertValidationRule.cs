// CertApi.Core/Validation/Framework/CertValidationRule.cs
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Core.Validation.Framework
{
    /// <summary>
    /// Base implementation of validation rule for specific entity types
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public abstract class CertValidationRule<T> : ICertValidationRule<T> where T : class
    {
        private readonly ILogger _logger;
        private readonly ICertValidationResultFactory _resultFactory;

        /// <summary>
        /// Gets the logger
        /// </summary>
        protected ILogger Logger => _logger;

        /// <summary>
        /// Gets the result factory
        /// </summary>
        protected ICertValidationResultFactory ResultFactory => _resultFactory;

        /// <summary>
        /// Gets the rule ID
        /// </summary>
        public string RuleId => GetType().Name;

        /// <summary>
        /// Gets the rule name
        /// </summary>
        public abstract string RuleName { get; }

        /// <summary>
        /// Gets the rule description
        /// </summary>
        public virtual string Description => $"Validates {typeof(T).Name} entities using {RuleName}";

        /// <summary>
        /// Initializes a new instance of the <see cref="CertValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Factory for creating validation results</param>
        protected CertValidationRule(
            ILogger logger,
            ICertValidationResultFactory resultFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        }

        /// <summary>
        /// Validates an entity
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        public virtual ICertValidationResult Validate(T entity, ICertValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                var result = ValidateInternal(entity, context);
                _logger.LogDebug(
                    "Validated {EntityType} with rule {RuleName}. Valid: {IsValid}, Errors: {ErrorCount}",
                    typeof(T).Name,
                    RuleName,
                    result.IsValid,
                    result.Errors.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error validating {EntityType} with rule {RuleName}",
                    typeof(T).Name,
                    RuleName);
                return _resultFactory.CreateInvalidResult(
                    context,
                    "ValidationException",
                    $"Error validating {typeof(T).Name} with rule {RuleName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates an entity asynchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public virtual async Task<ICertValidationResult> ValidateAsync(T entity, ICertValidationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                var result = await ValidateInternalAsync(entity, context, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug(
                    "Asynchronously validated {EntityType} with rule {RuleName}. Valid: {IsValid}, Errors: {ErrorCount}",
                    typeof(T).Name,
                    RuleName,
                    result.IsValid,
                    result.Errors.Count);
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Error validating {EntityType} with rule {RuleName} asynchronously",
                    typeof(T).Name,
                    RuleName);
                return _resultFactory.CreateInvalidResult(
                    context,
                    "ValidationException",
                    $"Error validating {typeof(T).Name} with rule {RuleName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates the entity internally
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        protected abstract ICertValidationResult ValidateInternal(T entity, ICertValidationContext context);

        /// <summary>
        /// Validates the entity internally asynchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        protected virtual async Task<ICertValidationResult> ValidateInternalAsync(
            T entity,
            ICertValidationContext context,
            CancellationToken cancellationToken)
        {
            // Default implementation calls synchronous version
            // Derived classes can override this for true async implementation
            return await Task.FromResult(ValidateInternal(entity, context)).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a valid result
        /// </summary>
        /// <param name="context">Validation context</param>
        /// <returns>Valid result</returns>
        protected ICertValidationResult CreateValidResult(ICertValidationContext context)
        {
            return _resultFactory.Success();
        }

        /// <summary>
        /// Creates an invalid result with an error code and message
        /// </summary>
        /// <param name="context">Validation context</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <returns>Invalid result</returns>
        protected ICertValidationResult CreateInvalidResult(
            ICertValidationContext context,
            string errorCode,
            string errorMessage)
        {
            return _resultFactory.CreateInvalidResult(context, errorCode, errorMessage);
        }

        /// <summary>
        /// Creates an invalid result with multiple validation errors
        /// </summary>
        /// <param name="context">Validation context</param>
        /// <param name="errors">Validation errors</param>
        /// <returns>Invalid result</returns>
        protected ICertValidationResult CreateInvalidResult(
            ICertValidationContext context,
            IEnumerable<CertValidationError> errors)
        {
            return _resultFactory.FromErrors(errors);
        }
    }
}