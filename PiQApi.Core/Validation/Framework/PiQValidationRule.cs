// PiQApi.Core/Validation/Framework/PiQValidationRule.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Core.Validation.Framework
{
    /// <summary>
    /// Base implementation of validation rule for specific entity types
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public abstract class PiQValidationRule<T> : IPiQValidationRule<T> where T : class
    {
        private readonly ILogger _logger;
        private readonly IPiQValidationResultFactory _resultFactory;

        /// <summary>
        /// Gets the logger
        /// </summary>
        protected ILogger Logger => _logger;

        /// <summary>
        /// Gets the result factory
        /// </summary>
        protected IPiQValidationResultFactory ResultFactory => _resultFactory;

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
        /// Initializes a new instance of the <see cref="PiQValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Factory for creating validation results</param>
        protected PiQValidationRule(
            ILogger logger,
            IPiQValidationResultFactory resultFactory)
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
        public virtual IPiQValidationResult Validate(T entity, IPiQValidationContext context)
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
        public virtual async Task<IPiQValidationResult> ValidateAsync(T entity, IPiQValidationContext context, CancellationToken cancellationToken = default)
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
        protected abstract IPiQValidationResult ValidateInternal(T entity, IPiQValidationContext context);

        /// <summary>
        /// Validates the entity internally asynchronously
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        protected virtual async Task<IPiQValidationResult> ValidateInternalAsync(
            T entity,
            IPiQValidationContext context,
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
        protected IPiQValidationResult CreateValidResult(IPiQValidationContext context)
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
        protected IPiQValidationResult CreateInvalidResult(
            IPiQValidationContext context,
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
        protected IPiQValidationResult CreateInvalidResult(
            IPiQValidationContext context,
            IEnumerable<PiQValidationError> errors)
        {
            return _resultFactory.FromErrors(errors);
        }
    }
}