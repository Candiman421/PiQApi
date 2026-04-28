// CertApi.Abstractions/Validation/ICertValidationProcessor.cs
using CertApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Abstractions.Validation
{
    /// <summary>
    /// Interface for validation processor
    /// </summary>
    public interface ICertValidationProcessor
    {
        /// <summary>
        /// Registers validation rules for a type
        /// </summary>
        void RegisterRules<T>(IEnumerable<ICertValidationRule<T>> rules) where T : class;

        /// <summary>
        /// Validates an entity
        /// </summary>
        ICertValidationResult Validate<T>(T entity, ICertValidationContext context) where T : class;

        /// <summary>
        /// Validates an entity asynchronously
        /// </summary>
        Task<ICertValidationResult> ValidateAsync<T>(T entity, ICertValidationContext context, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Validates an object asynchronously
        /// </summary>
        Task<ICertValidationResult> ValidateAsync(object entity, ICertValidationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates an object
        /// </summary>
        ICertValidationResult Validate(object entity, ICertValidationContext context);

        /// <summary>
        /// Validates an entity using specific validation rules
        /// </summary>
        ICertValidationResult ValidateWithRules<T>(T entity, IEnumerable<Type> ruleTypes, ICertValidationContext context) where T : class;

        /// <summary>
        /// Clears all registered rules
        /// </summary>
        void ClearRules();

        /// <summary>
        /// Gets the count of registered rules for a type
        /// </summary>
        int GetRuleCount<T>() where T : class;
    }
}