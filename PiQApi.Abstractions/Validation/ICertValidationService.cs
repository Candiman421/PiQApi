// PiQApi.Abstractions/Validation/ICertValidationService.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Abstractions.Validation
{
    /// <summary>
    /// Interface for validation service
    /// </summary>
    public interface ICertValidationService
    {
        /// <summary>
        /// Validates an entity using registered rules
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        ICertValidationResult Validate<T>(T entity, ICertValidationContext context) where T : class;

        /// <summary>
        /// Validates an entity asynchronously using registered rules
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<ICertValidationResult> ValidateAsync<T>(
            T entity,
            ICertValidationContext context,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Validates an entity using a specific set of rules
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="ruleTypes">Types of rules to use</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        ICertValidationResult ValidateWithRules<T>(
            T entity,
            IEnumerable<Type> ruleTypes,
            ICertValidationContext context) where T : class;

        /// <summary>
        /// Validates an entity using the specified options
        /// </summary>
        /// <param name="obj">Entity to validate</param>
        /// <param name="options">Validation options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<ICertValidationResult> ValidateAsync(
            object obj,
            object options,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates an entity using default options
        /// </summary>
        /// <param name="obj">Entity to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<ICertValidationResult> ValidateAsync(
            object obj,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a validation context with the specified correlation ID
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context</returns>
        ICertValidationContext CreateContext(
            string correlationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a validation context with a generated correlation ID
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context</returns>
        ICertValidationContext CreateContext(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a validation context with the specified validation mode
        /// </summary>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context</returns>
        ICertValidationContext CreateContext(
            ValidationModeType mode,
            CancellationToken cancellationToken = default);
    }
}