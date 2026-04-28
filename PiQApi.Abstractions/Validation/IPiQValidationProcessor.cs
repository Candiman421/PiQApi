// PiQApi.Abstractions/Validation/IPiQValidationProcessor.cs
using PiQApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Abstractions.Validation
{
    /// <summary>
    /// Interface for validation processor
    /// </summary>
    public interface IPiQValidationProcessor
    {
        /// <summary>
        /// Registers validation rules for a type
        /// </summary>
        void RegisterRules<T>(IEnumerable<IPiQValidationRule<T>> rules) where T : class;

        /// <summary>
        /// Validates an entity
        /// </summary>
        IPiQValidationResult Validate<T>(T entity, IPiQValidationContext context) where T : class;

        /// <summary>
        /// Validates an entity asynchronously
        /// </summary>
        Task<IPiQValidationResult> ValidateAsync<T>(T entity, IPiQValidationContext context, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Validates an object asynchronously
        /// </summary>
        Task<IPiQValidationResult> ValidateAsync(object entity, IPiQValidationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates an object
        /// </summary>
        IPiQValidationResult Validate(object entity, IPiQValidationContext context);

        /// <summary>
        /// Validates an entity using specific validation rules
        /// </summary>
        IPiQValidationResult ValidateWithRules<T>(T entity, IEnumerable<Type> ruleTypes, IPiQValidationContext context) where T : class;

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