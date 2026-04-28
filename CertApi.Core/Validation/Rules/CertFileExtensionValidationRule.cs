// CertApi.Core/Validation/Rules/CertFileExtensionValidationRule.cs
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using CertApi.Core.Validation.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace CertApi.Core.Validation.Rules
{
    /// <summary>
    /// Validation rule that validates file extensions for file paths
    /// </summary>
    /// <typeparam name="T">Entity type containing file paths</typeparam>
    public class CertFileExtensionValidationRule<T> : CertBaseValidationRule<T> where T : class
    {
        private readonly Func<T, string> _filePathGetter;
        private readonly string _propertyName;
        private readonly HashSet<string> _allowedExtensions;
        private readonly bool _ignoreCase;

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        public override string RuleName => "FileExtensionValidation";

        /// <summary>
        /// Initializes a new instance of the <see cref="CertFileExtensionValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Result factory</param>
        /// <param name="filePathExpression">Expression to get the file path</param>
        /// <param name="allowedExtensions">Allowed file extensions</param>
        /// <param name="ignoreCase">Whether to ignore case when checking extensions</param>
        public CertFileExtensionValidationRule(
            ILogger logger,
            ICertValidationResultFactory resultFactory,
            Expression<Func<T, string>> filePathExpression,
            IEnumerable<string> allowedExtensions,
            bool ignoreCase = true)
            : base(logger, resultFactory)
        {
            ArgumentNullException.ThrowIfNull(filePathExpression);
            ArgumentNullException.ThrowIfNull(allowedExtensions);

            _filePathGetter = filePathExpression.Compile();
            _propertyName = GetPropertyName(filePathExpression);
            _ignoreCase = ignoreCase;

            // Normalize extensions by ensuring they start with a period
            _allowedExtensions = new HashSet<string>(
                allowedExtensions.Select(NormalizeExtension),
                _ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets the property name from an expression
        /// </summary>
        /// <param name="expression">Expression to get the property</param>
        /// <returns>Property name</returns>
        private static string GetPropertyName(Expression<Func<T, string>> expression)
        {
            if (expression.Body is MemberExpression memberExpr)
            {
                return memberExpr.Member.Name;
            }

            if (expression.Body is UnaryExpression unaryExpr &&
                unaryExpr.Operand is MemberExpression memberExpr2)
            {
                return memberExpr2.Member.Name;
            }

            throw new ArgumentException("Expression must be a property access", nameof(expression));
        }

        /// <summary>
        /// Validates the file extension
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
                // Get the file path from the entity
                var filePath = _filePathGetter(entity);

                // If file path is null or empty, consider it valid (use a separate rule for required fields)
                if (string.IsNullOrEmpty(filePath))
                {
                    return ResultFactory.Success();
                }

                // Get the extension
                var extension = NormalizeExtension(Path.GetExtension(filePath));

                // Check if the extension is allowed
                if (_allowedExtensions.Contains(extension))
                {
                    return ResultFactory.Success();
                }

                // Create error message
                var allowedExtensionsStr = string.Join(", ", _allowedExtensions);
                var errorMessage = $"File extension '{extension}' is not allowed. Allowed extensions: {allowedExtensionsStr}";

                // Create error
                var error = CreateError(
                    _propertyName,
                    errorMessage,
                    "InvalidFileExtension");

                return ResultFactory.FromErrors(new[] { error });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating file extension for {EntityType}.{PropertyName}",
                    typeof(T).Name, _propertyName);
                return ResultFactory.FromException(ex);
            }
        }

        /// <summary>
        /// Normalizes an extension by ensuring it starts with a period
        /// </summary>
        /// <param name="extension">Extension to normalize</param>
        /// <returns>Normalized extension</returns>
        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return string.Empty;
            }

            return extension.StartsWith('.') ? extension : $".{extension}";
        }
    }
}