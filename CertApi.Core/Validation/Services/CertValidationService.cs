// CertApi.Core/Validation/Services/CertValidationService.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Utilities.Time;
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Core.Validation.Services
{
    /// <summary>
    /// Core implementation of validation service
    /// </summary>
    public class CertValidationService : ICertValidationService
    {
        private readonly ICertValidationProcessor _validationProcessor;
        private readonly ILogger<CertValidationService> _logger;
        private readonly ICertTimeProvider _timeProvider;

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, Type, Exception?> LogValidationStarted =
            LoggerMessage.Define<string, Type>(
                LogLevel.Debug,
                new EventId(1, "Validate"),
                "Starting validation with context ID {CorrelationId} for type {EntityType}");

        private static readonly Action<ILogger, string, Type, bool, int, Exception?> LogValidationCompleted =
            LoggerMessage.Define<string, Type, bool, int>(
                LogLevel.Debug,
                new EventId(2, "Validate"),
                "Completed validation with context ID {CorrelationId} for type {EntityType}. Success: {IsValid}, Errors: {ErrorCount}");

        private static readonly Action<ILogger, string, Type, int, Exception?> LogCustomRuleValidation =
            LoggerMessage.Define<string, Type, int>(
                LogLevel.Debug,
                new EventId(3, "ValidateWithRules"),
                "Validating with context ID {CorrelationId} for type {EntityType} using {RuleCount} custom rules");

        private static readonly Action<ILogger, Type, Exception?> LogGenericValidationStarted =
            LoggerMessage.Define<Type>(
                LogLevel.Debug,
                new EventId(4, "ValidateAsync"),
                "Starting validation for object type {EntityType}");

        /// <summary>
        /// Initializes a new instance of the <see cref="CertValidationService"/> class
        /// </summary>
        /// <param name="validationProcessor">Validation processor</param>
        /// <param name="logger">Logger</param>
        /// <param name="timeProvider">Time provider</param>
        public CertValidationService(
            ICertValidationProcessor validationProcessor,
            ILogger<CertValidationService> logger,
            ICertTimeProvider timeProvider)
        {
            _validationProcessor = validationProcessor ?? throw new ArgumentNullException(nameof(validationProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        /// <summary>
        /// Validates an entity using the specified validation context
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        public ICertValidationResult Validate<T>(T entity, ICertValidationContext context) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            LogValidationStarted(_logger, context.CorrelationId, typeof(T), null);

            var result = _validationProcessor.Validate(entity, context);

            LogValidationCompleted(
                _logger,
                context.CorrelationId,
                typeof(T),
                result.IsValid,
                result.Errors.Count,
                null);

            return result;
        }

        /// <summary>
        /// Validates an entity using the specified validation context asynchronously
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Validation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public async Task<ICertValidationResult> ValidateAsync<T>(
            T entity,
            ICertValidationContext context,
            CancellationToken cancellationToken = default) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            // Merge cancellation tokens
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                context.CancellationToken,
                cancellationToken);

            LogValidationStarted(_logger, context.CorrelationId, typeof(T), null);

            var result = await _validationProcessor.ValidateAsync(entity, context, combinedCts.Token)
                .ConfigureAwait(false);

            LogValidationCompleted(
                _logger,
                context.CorrelationId,
                typeof(T),
                result.IsValid,
                result.Errors.Count,
                null);

            return result;
        }

        /// <summary>
        /// Validates an entity using specific validation rules
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="ruleTypes">Types of validation rules to use</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result</returns>
        public ICertValidationResult ValidateWithRules<T>(
            T entity,
            IEnumerable<Type> ruleTypes,
            ICertValidationContext context) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(ruleTypes);
            ArgumentNullException.ThrowIfNull(context);

            var ruleTypesList = ruleTypes.ToList();
            LogCustomRuleValidation(_logger, context.CorrelationId, typeof(T), ruleTypesList.Count, null);

            // Use the processor to validate with specific rules
            return _validationProcessor.ValidateWithRules(entity, ruleTypesList, context);
        }

        /// <summary>
        /// Validates an entity using the specified options
        /// </summary>
        /// <param name="obj">Entity to validate</param>
        /// <param name="options">Validation options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public async Task<ICertValidationResult> ValidateAsync(
            object obj,
            object options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(options);

            LogGenericValidationStarted(_logger, obj.GetType(), null);

            // Create a validation context from the options
            var context = CreateContextFromOptions(options, cancellationToken);

            // Validate using the processor
            return await _validationProcessor.ValidateAsync(obj, context, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Validates an entity using default options
        /// </summary>
        /// <param name="obj">Entity to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        public async Task<ICertValidationResult> ValidateAsync(
            object obj,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(obj);

            LogGenericValidationStarted(_logger, obj.GetType(), null);

            // Create a default validation context
            var context = CreateContext(cancellationToken);

            // Validate using the processor
            return await _validationProcessor.ValidateAsync(obj, context, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new validation context with standard settings
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A validation context</returns>
        public ICertValidationContext CreateContext(
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            return new CertValidationContext(
                ValidationModeType.Standard, // Default mode
                0, // Initial depth
                10, // Default max depth
                false, // Don't aggregate errors by default
                correlationId,
                ImmutableDictionary<string, object>.Empty,
                _timeProvider,
                cancellationToken);
        }

        /// <summary>
        /// Creates a new validation context with standard settings and a generated correlation ID
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A validation context</returns>
        public ICertValidationContext CreateContext(
            CancellationToken cancellationToken = default)
        {
            // Generate a new correlation ID
            string correlationId = Guid.NewGuid().ToString("N");
            return CreateContext(correlationId, cancellationToken);
        }

        /// <summary>
        /// Creates a new validation context with the specified validation mode
        /// </summary>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A validation context</returns>
        public ICertValidationContext CreateContext(
            ValidationModeType mode,
            CancellationToken cancellationToken = default)
        {
            // Generate a new correlation ID
            string correlationId = Guid.NewGuid().ToString("N");
            return new CertValidationContext(
                mode,
                0, // Initial depth
                10, // Default max depth
                mode == ValidationModeType.Lenient, // Aggregate errors in lenient mode
                correlationId,
                ImmutableDictionary<string, object>.Empty,
                _timeProvider,
                cancellationToken);
        }

        /// <summary>
        /// Creates a validation context from options
        /// </summary>
        /// <param name="options">Options object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A validation context</returns>
        private ICertValidationContext CreateContextFromOptions(object options, CancellationToken cancellationToken)
        {
            // Extract properties via reflection
            var properties = options.GetType().GetProperties();
            var dictionary = new Dictionary<string, object>();

            // Default values
            var mode = ValidationModeType.Standard;
            var correlationId = Guid.NewGuid().ToString("N");
            var aggregateErrors = false;
            var maxDepth = 10;

            // Look for specific properties
            foreach (var property in properties)
            {
                var value = property.GetValue(options);
                if (value != null)
                {
                    if (property.Name.Equals("Mode", StringComparison.OrdinalIgnoreCase) && value is ValidationModeType modeValue)
                    {
                        mode = modeValue;
                        aggregateErrors = mode == ValidationModeType.Lenient;
                    }
                    else if (property.Name.Equals("CorrelationId", StringComparison.OrdinalIgnoreCase) && value is string correlationIdValue)
                    {
                        correlationId = correlationIdValue;
                    }
                    else if (property.Name.Equals("MaxDepth", StringComparison.OrdinalIgnoreCase) && value is int maxDepthValue)
                    {
                        maxDepth = maxDepthValue;
                    }
                    else if (property.Name.Equals("AggregateErrors", StringComparison.OrdinalIgnoreCase) && value is bool aggregateErrorsValue)
                    {
                        aggregateErrors = aggregateErrorsValue;
                    }
                    else
                    {
                        // Add other properties to the context dictionary
                        dictionary[property.Name] = value;
                    }
                }
            }

            return new CertValidationContext(
                mode,
                0, // Initial depth
                maxDepth,
                aggregateErrors,
                correlationId,
                dictionary.ToImmutableDictionary(),
                _timeProvider,
                cancellationToken);
        }
    }
}