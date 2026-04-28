// PiQApi.Core/Validation/Rules/CertHeaderValidationRule.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Core.Validation.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace PiQApi.Core.Validation.Rules
{
    /// <summary>
    /// Validation rule that checks file headers or content headers
    /// </summary>
    /// <typeparam name="T">Entity type containing header data</typeparam>
    public class CertHeaderValidationRule<T> : CertBaseValidationRule<T> where T : class
    {
        private readonly Func<T, string> _contentGetter;
        private readonly string _propertyName;
        private readonly IReadOnlyList<string> _requiredHeaders;
        private readonly bool _exactMatch;
        private readonly bool _ignoreCase;
        private readonly StringComparison _comparison;

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        public override string RuleName => "HeaderValidation";

        /// <summary>
        /// Initializes a new instance of the <see cref="CertHeaderValidationRule{T}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resultFactory">Result factory</param>
        /// <param name="contentExpression">Expression to get the content</param>
        /// <param name="requiredHeaders">Required headers</param>
        /// <param name="exactMatch">Whether headers must match exactly</param>
        /// <param name="ignoreCase">Whether to ignore case</param>
        public CertHeaderValidationRule(
            ILogger logger,
            ICertValidationResultFactory resultFactory,
            Expression<Func<T, string>> contentExpression,
            IEnumerable<string> requiredHeaders,
            bool exactMatch = false,
            bool ignoreCase = true)
            : base(logger, resultFactory)
        {
            ArgumentNullException.ThrowIfNull(contentExpression);
            ArgumentNullException.ThrowIfNull(requiredHeaders);

            _contentGetter = contentExpression.Compile();
            _propertyName = GetPropertyName(contentExpression);
            _requiredHeaders = requiredHeaders.ToList().AsReadOnly();
            _exactMatch = exactMatch;
            _ignoreCase = ignoreCase;
            _comparison = ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
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
        /// Validates the headers
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
                // Get the content from the entity
                var content = _contentGetter(entity);

                // If content is null or empty, consider it invalid
                if (string.IsNullOrEmpty(content))
                {
                    var error = CreateError(
                        _propertyName,
                        "Content is null or empty",
                        "EmptyContent");

                    return ResultFactory.FromErrors(new[] { error });
                }

                // Read the first few lines (to check headers)
                var headerLines = ReadHeaderLines(content, 10); // Read up to 10 lines

                // Find missing headers
                var missingHeaders = new List<string>();
                foreach (var requiredHeader in _requiredHeaders)
                {
                    if (!headerLines.Any(line => ContainsHeader(line, requiredHeader)))
                    {
                        missingHeaders.Add(requiredHeader);
                    }
                }

                // If exact match, check that no extra headers exist
                if (_exactMatch && headerLines.Count > _requiredHeaders.Count)
                {
                    var error = CreateError(
                        _propertyName,
                        "Content contains extra headers that are not allowed with exact matching",
                        "ExtraHeaders");

                    return ResultFactory.FromErrors(new[] { error });
                }

                // Return success if no missing headers
                if (missingHeaders.Count == 0)
                {
                    return ResultFactory.Success();
                }

                // Create error for missing headers
                var missingHeadersStr = string.Join(", ", missingHeaders);
                var errorMessage = $"Required headers not found: {missingHeadersStr}";

                var headerError = CreateError(
                    _propertyName,
                    errorMessage,
                    "MissingHeaders");

                return ResultFactory.FromErrors(new[] { headerError });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating headers for {EntityType}.{PropertyName}",
                    typeof(T).Name, _propertyName);
                return ResultFactory.FromException(ex);
            }
        }

        /// <summary>
        /// Reads the header lines from content
        /// </summary>
        /// <param name="content">Content to read</param>
        /// <param name="maxLines">Maximum number of lines to read</param>
        /// <returns>List of header lines</returns>
        private List<string> ReadHeaderLines(string content, int maxLines)
        {
            var result = new List<string>();

            using (var reader = new StringReader(content))
            {
                string? line;
                int lineCount = 0;

                while ((line = reader.ReadLine()) != null && lineCount < maxLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        result.Add(line);
                    }

                    lineCount++;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a line contains a header
        /// </summary>
        /// <param name="line">Line to check</param>
        /// <param name="header">Header to find</param>
        /// <returns>True if the line contains the header</returns>
        private bool ContainsHeader(string line, string header)
        {
            return line.Contains(header, _comparison);
        }
    }
}