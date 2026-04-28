// PiQApi.Core/Validation/Rules/CertCompositeValidationRule.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Core.Validation.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Core.Validation.Rules
{
    /// <summary>
    /// Composite validation rule that applies multiple child rules
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public class CertCompositeValidationRule<T> : CertBaseValidationRule<T> where T : class
    {
        private readonly List<ICertValidationRule<T>> _childRules = new();

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        public override string RuleName => "CompositeValidation";

        /// <summary>
        /// Initializes a new instance of the <see cref="CertCompositeValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Validation result factory</param>
        public CertCompositeValidationRule(
            ILogger logger,
            ICertValidationResultFactory resultFactory)
            : base(logger, resultFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertCompositeValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Validation result factory</param>
        /// <param name="childRules">Child rules to apply</param>
        public CertCompositeValidationRule(
            ILogger logger,
            ICertValidationResultFactory resultFactory,
            IEnumerable<ICertValidationRule<T>> childRules)
            : base(logger, resultFactory)
        {
            ArgumentNullException.ThrowIfNull(childRules);

            _childRules.AddRange(childRules);
        }

        /// <summary>
        /// Adds a child rule
        /// </summary>
        /// <param name="rule">Rule to add</param>
        /// <returns>This rule for fluent chaining</returns>
        public CertCompositeValidationRule<T> AddRule(ICertValidationRule<T> rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            _childRules.Add(rule);
            return this;
        }

        /// <summary>
        /// Adds multiple child rules
        /// </summary>
        /// <param name="rules">Rules to add</param>
        /// <returns>This rule for fluent chaining</returns>
        public CertCompositeValidationRule<T> AddRules(IEnumerable<ICertValidationRule<T>> rules)
        {
            ArgumentNullException.ThrowIfNull(rules);

            _childRules.AddRange(rules);
            return this;
        }

        /// <summary>
        /// Gets the child rules
        /// </summary>
        /// <returns>Enumerable of child rules</returns>
        public IEnumerable<ICertValidationRule<T>> GetRules() => _childRules.AsReadOnly();

        /// <summary>
        /// Validates the entity using all child rules
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        protected override ICertValidationResult ValidateInternal(T entity, ICertValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            if (_childRules.Count == 0)
            {
                // No rules to apply, return success
                return ResultFactory.Success();
            }

            // Create list to collect all errors
            var errors = new List<CertValidationError>();

            // Apply each rule in sequence
            foreach (var rule in _childRules)
            {
                var result = rule.Validate(entity, context);

                if (!result.IsValid)
                {
                    // Add all errors from this rule
                    errors.AddRange(result.Errors);

                    // Stop on first failure if not aggregating errors
                    if (!context.AggregateAllErrors)
                    {
                        break;
                    }
                }
            }

            // Return success if no errors, otherwise return errors
            return errors.Count > 0
                ? ResultFactory.FromErrors(errors)
                : ResultFactory.Success();
        }

        /// <summary>
        /// Validates the entity asynchronously using all child rules
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        protected override async Task<ICertValidationResult> ValidateInternalAsync(
            T entity,
            ICertValidationContext context,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            if (_childRules.Count == 0)
            {
                // No rules to apply, return success
                return ResultFactory.Success();
            }

            // Create list to collect all errors
            var errors = new List<CertValidationError>();

            // Apply each rule in sequence
            foreach (var rule in _childRules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Call async validation if available, otherwise call sync validation
                ICertValidationResult result;
                if (rule is ICertAsyncValidationRule<T> asyncRule)
                {
                    result = await asyncRule.ValidateAsync(entity, context, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    result = rule.Validate(entity, context);
                }

                if (!result.IsValid)
                {
                    // Add all errors from this rule
                    errors.AddRange(result.Errors);

                    // Stop on first failure if not aggregating errors
                    if (!context.AggregateAllErrors)
                    {
                        break;
                    }
                }
            }

            // Return success if no errors, otherwise return errors
            return errors.Count > 0
                ? ResultFactory.FromErrors(errors)
                : ResultFactory.Success();
        }
    }
}