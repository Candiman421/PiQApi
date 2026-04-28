// PiQApi.Core/Validation/Rules/PiQCompositeValidationRule.cs
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
    public class PiQCompositeValidationRule<T> : PiQBaseValidationRule<T> where T : class
    {
        private readonly List<IPiQValidationRule<T>> _childRules = new();

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        public override string RuleName => "CompositeValidation";

        /// <summary>
        /// Initializes a new instance of the <see cref="PiQCompositeValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Validation result factory</param>
        public PiQCompositeValidationRule(
            ILogger logger,
            IPiQValidationResultFactory resultFactory)
            : base(logger, resultFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PiQCompositeValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Validation result factory</param>
        /// <param name="childRules">Child rules to apply</param>
        public PiQCompositeValidationRule(
            ILogger logger,
            IPiQValidationResultFactory resultFactory,
            IEnumerable<IPiQValidationRule<T>> childRules)
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
        public PiQCompositeValidationRule<T> AddRule(IPiQValidationRule<T> rule)
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
        public PiQCompositeValidationRule<T> AddRules(IEnumerable<IPiQValidationRule<T>> rules)
        {
            ArgumentNullException.ThrowIfNull(rules);

            _childRules.AddRange(rules);
            return this;
        }

        /// <summary>
        /// Gets the child rules
        /// </summary>
        /// <returns>Enumerable of child rules</returns>
        public IEnumerable<IPiQValidationRule<T>> GetRules() => _childRules.AsReadOnly();

        /// <summary>
        /// Validates the entity using all child rules
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        protected override IPiQValidationResult ValidateInternal(T entity, IPiQValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            if (_childRules.Count == 0)
            {
                // No rules to apply, return success
                return ResultFactory.Success();
            }

            // Create list to collect all errors
            var errors = new List<PiQValidationError>();

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
        protected override async Task<IPiQValidationResult> ValidateInternalAsync(
            T entity,
            IPiQValidationContext context,
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
            var errors = new List<PiQValidationError>();

            // Apply each rule in sequence
            foreach (var rule in _childRules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Call async validation if available, otherwise call sync validation
                IPiQValidationResult result;
                if (rule is IPiQAsyncValidationRule<T> asyncRule)
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