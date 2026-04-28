// PiQApi.Abstractions/Factories/IPiQValidationResultFactory.cs
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
    public interface IPiQValidationResultFactory
    {
        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        IPiQValidationResult Success();

        /// <summary>
        /// Creates a validation result from a single error
        /// </summary>
        IPiQValidationResult FromError(string propertyName, string errorMessage, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error);

        /// <summary>
        /// Creates a validation result from a collection of errors
        /// </summary>
        IPiQValidationResult FromErrors(IEnumerable<PiQValidationError> errors);

        /// <summary>
        /// Creates a validation result from an exception
        /// </summary>
        IPiQValidationResult FromException(Exception exception);

        /// <summary>
        /// Combines multiple validation results
        /// </summary>
        IPiQValidationResult Combine(params IPiQValidationResult[] results);

        /// <summary>
        /// Creates a cancelled validation result
        /// </summary>
        IPiQValidationResult CreateCancelled(string correlationId);

        /// <summary>
        /// Creates an invalid validation result
        /// </summary>
        /// <param name="context">Validation context</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="propertyName">Optional property name</param>
        /// <param name="severity">Validation severity</param>
        /// <returns>Invalid validation result</returns>
        IPiQValidationResult CreateInvalidResult(
            IPiQValidationContext context,
            string errorCode,
            string errorMessage,
            string propertyName = "",
            ValidationSeverityType severity = ValidationSeverityType.Error);

        /// <summary>
        /// Creates a validation result builder
        /// </summary>
        PiQValidationResultBuilder CreateBuilder(string? correlationId = null);
    }
}