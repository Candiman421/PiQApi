// PiQApi.Core/Validation/PiQValidationResultFactory.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiQApi.Core.Validation
{
    /// <summary>
    /// Factory for creating validation results
    /// </summary>
    public class PiQValidationResultFactory : IPiQValidationResultFactory
    {
        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public IPiQValidationResult Success()
        {
            return new PiQCoreValidationResult();
        }

        /// <summary>
        /// Creates a validation result from a single error
        /// </summary>
        public IPiQValidationResult FromError(string propertyName, string errorMessage, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            return new PiQCoreValidationResult(
                new[] { new PiQValidationError(propertyName, errorMessage, errorCode, severity) });
        }

        /// <summary>
        /// Creates a validation result from a collection of errors
        /// </summary>
        public IPiQValidationResult FromErrors(IEnumerable<PiQValidationError> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            return new PiQCoreValidationResult(errors);
        }

        /// <summary>
        /// Creates a validation result from an exception
        /// </summary>
        public IPiQValidationResult FromException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return new PiQCoreValidationResult(
                new[] { new PiQValidationError("Exception", exception.Message, "ExceptionError") },
                exception);
        }

        /// <summary>
        /// Combines multiple validation results
        /// </summary>
        public IPiQValidationResult Combine(params IPiQValidationResult[] results)
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

            return new PiQCoreValidationResult(allErrors, allExceptions, context);
        }

        /// <summary>
        /// Creates a cancelled validation result
        /// </summary>
        public IPiQValidationResult CreateCancelled(string correlationId)
        {
            var context = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(correlationId))
            {
                context["CorrelationId"] = correlationId;
            }

            return new PiQCoreValidationResult(
                new[] { new PiQValidationError(string.Empty, "Validation was cancelled", "Cancelled", ValidationSeverityType.Warning) },
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
        public IPiQValidationResult CreateInvalidResult(
            IPiQValidationContext context,
            string errorCode,
            string errorMessage,
            string propertyName = "",
            ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Create a validation error
            var error = new PiQValidationError(
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
            return new PiQCoreValidationResult(new[] { error }, null, contextDict);
        }

        /// <summary>
        /// Creates a validation result builder
        /// </summary>
        public PiQValidationResultBuilder CreateBuilder(string? correlationId = null)
        {
            return new PiQValidationResultBuilder(this, correlationId);
        }
    }
}