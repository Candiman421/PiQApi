// CertApi.Core/Validation/CertValidationResultFactory.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CertApi.Core.Validation
{
    /// <summary>
    /// Factory for creating validation results
    /// </summary>
    public class CertValidationResultFactory : ICertValidationResultFactory
    {
        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public ICertValidationResult Success()
        {
            return new CertCoreValidationResult();
        }

        /// <summary>
        /// Creates a validation result from a single error
        /// </summary>
        public ICertValidationResult FromError(string propertyName, string errorMessage, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            return new CertCoreValidationResult(
                new[] { new CertValidationError(propertyName, errorMessage, errorCode, severity) });
        }

        /// <summary>
        /// Creates a validation result from a collection of errors
        /// </summary>
        public ICertValidationResult FromErrors(IEnumerable<CertValidationError> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            return new CertCoreValidationResult(errors);
        }

        /// <summary>
        /// Creates a validation result from an exception
        /// </summary>
        public ICertValidationResult FromException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return new CertCoreValidationResult(
                new[] { new CertValidationError("Exception", exception.Message, "ExceptionError") },
                exception);
        }

        /// <summary>
        /// Combines multiple validation results
        /// </summary>
        public ICertValidationResult Combine(params ICertValidationResult[] results)
        {
            ArgumentNullException.ThrowIfNull(results);
            var allErrors = results.SelectMany(r => r.Errors).ToList();
            var allExceptions = results.Select(r => r.Exception).FirstOrDefault(e => e != null);
            var context = new Dictionary<string, object>();

            // Merge context from all results
            foreach (var result in results)
            {
                foreach (var kvp in result.Context)
                {
                    context[kvp.Key] = kvp.Value;
                }
            }

            return new CertCoreValidationResult(allErrors, allExceptions, context);
        }

        /// <summary>
        /// Creates a cancelled validation result
        /// </summary>
        public ICertValidationResult CreateCancelled(string correlationId)
        {
            var context = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(correlationId))
            {
                context["CorrelationId"] = correlationId;
            }

            return new CertCoreValidationResult(
                new[] { new CertValidationError(string.Empty, "Validation was cancelled", "Cancelled", ValidationSeverityType.Warning) },
                null,
                context);
        }

        /// <summary>
        /// Creates an invalid validation result
        /// </summary>
        /// <param name="context">Validation context</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="propertyName">Optional property name</param>
        /// <param name="severity">Validation severity</param>
        /// <returns>Invalid validation result</returns>
        public ICertValidationResult CreateInvalidResult(
            ICertValidationContext context,
            string errorCode,
            string errorMessage,
            string propertyName = "",
            ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Create a validation error
            var error = new CertValidationError(
                propertyName,
                errorMessage,
                errorCode,
                severity);

            // Create context dictionary with correlation ID
            var contextDict = new Dictionary<string, object>
            {
                ["CorrelationId"] = context.CorrelationId
            };

            // Use FromErrors to create the result
            return new CertCoreValidationResult(new[] { error }, null, contextDict);
        }

        /// <summary>
        /// Creates a validation result builder
        /// </summary>
        public CertValidationResultBuilder CreateBuilder(string? correlationId = null)
        {
            return new CertValidationResultBuilder(this, correlationId);
        }
    }
}