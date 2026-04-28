// CertApi.Core/Validation/Rules/CertPropertyValidationRule.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using CertApi.Core.Validation.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Core.Validation.Rules
{
    /// <summary>
    /// Rule that validates a specific property of an entity
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    public class CertPropertyValidationRule<T> : CertBaseValidationRule<T> where T : class
    {
        private readonly string _propertyName;
        private readonly Func<T, object?> _propertyGetter;
        private readonly Func<object?, bool> _predicate;
        private readonly string _errorMessage;
        private readonly string _errorCode;
        private readonly ValidationSeverityType _severity;

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        public override string RuleName => $"PropertyValidation_{_propertyName}";

        /// <summary>
        /// Initializes a new instance of the <see cref="CertPropertyValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Validation result factory</param>
        /// <param name="propertyExpression">Expression to access the property</param>
        /// <param name="predicate">Predicate for validation</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="severity">Validation severity</param>
        public CertPropertyValidationRule(
            ILogger logger,
            ICertValidationResultFactory resultFactory,
            Expression<Func<T, object?>> propertyExpression,
            Func<object?, bool> predicate,
            string errorMessage,
            string errorCode = "PropertyValidationFailed",
            ValidationSeverityType severity = ValidationSeverityType.Error)
            : base(logger, resultFactory)
        {
            ArgumentNullException.ThrowIfNull(propertyExpression);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentException.ThrowIfNullOrEmpty(errorMessage);

            _propertyName = GetPropertyName(propertyExpression);
            _propertyGetter = propertyExpression.Compile();
            _predicate = predicate;
            _errorMessage = errorMessage;
            _errorCode = errorCode;
            _severity = severity;
        }

        /// <summary>
        /// Gets the property name from an expression
        /// </summary>
        /// <param name="propertyExpression">Expression to access the property</param>
        /// <returns>Property name</returns>
        private static string GetPropertyName(Expression<Func<T, object?>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpr)
            {
                return memberExpr.Member.Name;
            }

            if (propertyExpression.Body is UnaryExpression unaryExpr &&
                unaryExpr.Operand is MemberExpression memberExpr2)
            {
                return memberExpr2.Member.Name;
            }

            throw new ArgumentException("Expression must be a property access", nameof(propertyExpression));
        }

        /// <summary>
        /// Validates the property of the entity
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        protected override ICertValidationResult ValidateInternal(T entity, ICertValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                // Get the property value
                var propertyValue = _propertyGetter(entity);

                // Apply the predicate to validate
                if (_predicate(propertyValue))
                {
                    // Validation succeeded
                    Logger.LogDebug("Property {PropertyName} validation succeeded for {EntityType}",
                        _propertyName, typeof(T).Name);
                    return ResultFactory.Success();
                }
                else
                {
                    // Validation failed
                    Logger.LogDebug("Property {PropertyName} validation failed for {EntityType}: {ErrorMessage}",
                        _propertyName, typeof(T).Name, _errorMessage);

                    // Create error with property name
                    var error = CreateError(_propertyName, _errorMessage, _errorCode, _severity);
                    return ResultFactory.FromErrors(new[] { error });
                }
            }
            catch (Exception ex)
            {
                // Log and wrap any exceptions
                Logger.LogError(ex, "Error validating property {PropertyName} for {EntityType}",
                    _propertyName, typeof(T).Name);

                return ResultFactory.FromException(ex);
            }
        }

        /// <summary>
        /// Validates the property of the entity asynchronously
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
            // For simple property validation, we just defer to synchronous validation
            // This could be overridden in derived classes for truly async validation
            await Task.Yield(); // Make sure this is actually async

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Perform the validation
                return ValidateInternal(entity, context);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Property {PropertyName} validation for {EntityType} was cancelled",
                    _propertyName, typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                // Log and wrap any exceptions
                Logger.LogError(ex, "Error validating property {PropertyName} for {EntityType} asynchronously",
                    _propertyName, typeof(T).Name);

                return ResultFactory.FromException(ex);
            }
        }
    }
}
