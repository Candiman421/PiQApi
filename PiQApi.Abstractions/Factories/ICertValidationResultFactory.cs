// PiQApi.Abstractions/Factories/ICertValidationResultFactory.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;

namespace PiQApi.Abstractions.Factories
{
    /// <summary>
    /// Factory for creating validation results
    /// </summary>
    public interface ICertValidationResultFactory
    {
        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        ICertValidationResult Success();

        /// <summary>
        /// Creates a validation result from a single error
        /// </summary>
        ICertValidationResult FromError(string propertyName, string errorMessage, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error);

        /// <summary>
        /// Creates a validation result from a collection of errors
        /// </summary>
        ICertValidationResult FromErrors(IEnumerable<CertValidationError> errors);

        /// <summary>
        /// Creates a validation result from an exception
        /// </summary>
        ICertValidationResult FromException(Exception exception);

        /// <summary>
        /// Combines multiple validation results
        /// </summary>
        ICertValidationResult Combine(params ICertValidationResult[] results);

        /// <summary>
        /// Creates a cancelled validation result
        /// </summary>
        ICertValidationResult CreateCancelled(string correlationId);

        /// <summary>
        /// Creates an invalid validation result
        /// </summary>
        /// <param name="context">Validation context</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="propertyName">Optional property name</param>
        /// <param name="severity">Validation severity</param>
        /// <returns>Invalid validation result</returns>
        ICertValidationResult CreateInvalidResult(
            ICertValidationContext context,
            string errorCode,
            string errorMessage,
            string propertyName = "",
            ValidationSeverityType severity = ValidationSeverityType.Error);

        /// <summary>
        /// Creates a validation result builder
        /// </summary>
        CertValidationResultBuilder CreateBuilder(string? correlationId = null);
    }
}