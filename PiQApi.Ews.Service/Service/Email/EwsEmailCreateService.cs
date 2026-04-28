// PiQApi.Ews.Service/Service/Email/EwsEmailCreateService.cs
using System.Diagnostics;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Core.Exceptions.Infrastructure;
using PiQApi.Ews.Core.Enums;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Core.Validation.Interfaces;
using PiQApi.Ews.Service.Core;
using PiQApi.Ews.Service.Core.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace PiQApi.Ews.Service.Service.Email
{
    /// <summary>
    /// Implementation of email creation service for Exchange
    /// </summary>
    public class EwsEmailCreateService : EwsServiceBase, IEwsEmailCreateService
    {
        private readonly IEwsPropertySetService _propertySetService;
        private readonly IEwsValidationContextFactory _validationContextFactory;
        private readonly IPiQValidationProcessor _validationProcessor;
        private readonly ILogger<EwsEmailCreateService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsEmailCreateService"/> class
        /// </summary>
        public EwsEmailCreateService(
            IExchangeServiceWrapper serviceWrapper,
            IEwsErrorMappingService errorMappingService,
            IEwsPolicyExecutor policyExecutor,
            IPiQExceptionFactory exceptionFactory,
            IEwsPropertySetService propertySetService,
            IEwsValidationContextFactory validationContextFactory,
            IPiQValidationProcessor validationProcessor,
            ILogger<EwsEmailCreateService> logger)
            : base(serviceWrapper, errorMappingService, policyExecutor, exceptionFactory, logger)
        {
            _propertySetService = propertySetService ?? throw new ArgumentNullException(nameof(propertySetService));
            _validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
            _validationProcessor = validationProcessor ?? throw new ArgumentNullException(nameof(validationProcessor));
            _logger = logger;
        }

        /// <summary>
        /// Creates a new email message
        /// </summary>
        public async Task<EmailMessage> CreateMessageAsync(IEwsOperationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            _logger.LogDebug("Creating new email message. CorrelationId: {CorrelationId}", context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Execute through policy executor to create message
                var message = await ExecuteAsync(
                    context,
                    service => Task.FromResult(new EmailMessage(service)),
                    EwsPolicyType.Mail,
                    nameof(CreateMessageAsync),
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("CreateMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsCreated");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return message;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("CreateMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Creates a new email message with a specific property set
        /// </summary>
        public async Task<EmailMessage> CreateMessageWithPropertySetAsync(IEwsOperationContext context, PropertySet propertySet, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(propertySet);

            _logger.LogDebug("Creating new email message with property set. CorrelationId: {CorrelationId}", context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate property set first
                await ValidatePropertySetAsync(propertySet, context, cancellationToken).ConfigureAwait(false);

                // First operation: Get enhanced property set if needed
                var enhancedPropertySet = await PolicyExecutor.ExecuteAsync(
                    async () => await _propertySetService.WithEmailPropertiesAsync(propertySet, cancellationToken).ConfigureAwait(false),
                    EwsPolicyType.Service,
                    nameof(CreateMessageWithPropertySetAsync) + "_GetPropertySet",
                    cancellationToken).ConfigureAwait(false);

                // Second operation: Create message with property set
                var message = await ExecuteAsync(
                    context,
                    async service =>
                    {
                        var msg = new EmailMessage(service);
                        await msg.Load(enhancedPropertySet).ConfigureAwait(false);
                        return msg;
                    },
                    EwsPolicyType.Mail,
                    nameof(CreateMessageWithPropertySetAsync) + "_CreateWithPropertySet",
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("CreateMessageWithPropertySet", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsCreatedWithPropertySet");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return message;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("CreateMessageWithPropertySet", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Saves a message as a draft
        /// </summary>
        public async Task SaveDraftAsync(IEwsOperationContext context, EmailMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(message);

            _logger.LogDebug("Saving message as draft. CorrelationId: {CorrelationId}", context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate message before saving
                await ValidateMessageAsync(message, context, cancellationToken).ConfigureAwait(false);

                // Save the message using policy executor
                await ExecuteAsync(
                    context,
                    async service =>
                    {
                        await message.Save(WellKnownFolderName.Drafts).ConfigureAwait(false);
                    },
                    EwsPolicyType.Mail,
                    nameof(SaveDraftAsync),
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("SaveDraft", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsSaved");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("SaveDraft", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Validates a property set
        /// </summary>
        private async Task ValidatePropertySetAsync(PropertySet propertySet, IEwsOperationContext context, CancellationToken cancellationToken)
        {
            try
            {
                // Create validation context with Exchange server version
                var validationContext = _validationContextFactory.CreateWithScope(
                    ServiceWrapper.Service.RequestedServerVersion,
                    "MessagePropertySet",
                    cancellationToken);

                // Add correlation ID for tracking
                validationContext = validationContext.WithValue("CorrelationId", context.CorrelationId);

                // Execute validation using policy to handle transient failures
                var validationResult = await PolicyExecutor.ExecuteAsync(
                    async () => await _validationProcessor.ValidateAsync(
                        propertySet,
                        validationContext,
                        cancellationToken).ConfigureAwait(false),
                    EwsPolicyType.Validation,
                    nameof(ValidatePropertySetAsync),
                    cancellationToken).ConfigureAwait(false);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Property set validation failed. CorrelationId: {CorrelationId}, Errors: {ErrorCount}",
                        context.CorrelationId, validationResult.Errors.Count);

                    var exception = new PiQValidationException("Property set validation failed", validationResult.Errors);
                    exception.SetCorrelationId(context.CorrelationId);
                    throw exception;
                }
            }
            catch (PiQException ex)
            {
                ex.SetCorrelationId(context.CorrelationId);
                throw;
            }
        }

        /// <summary>
        /// Validates a message
        /// </summary>
        private async Task ValidateMessageAsync(EmailMessage message, IEwsOperationContext context, CancellationToken cancellationToken)
        {
            try
            {
                // Create validation context with Exchange server version
                var validationContext = _validationContextFactory.CreateWithScope(
                    ServiceWrapper.Service.RequestedServerVersion,
                    "MessageDraft",
                    cancellationToken);

                // Add correlation ID for tracking
                validationContext = validationContext.WithValue("CorrelationId", context.CorrelationId);

                // Execute validation using policy to handle transient failures
                var validationResult = await PolicyExecutor.ExecuteAsync(
                    async () => await _validationProcessor.ValidateAsync(
                        message,
                        validationContext,
                        cancellationToken).ConfigureAwait(false),
                    EwsPolicyType.Validation,
                    nameof(ValidateMessageAsync),
                    cancellationToken).ConfigureAwait(false);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Message validation failed. CorrelationId: {CorrelationId}, Errors: {ErrorCount}",
                        context.CorrelationId, validationResult.Errors.Count);

                    var exception = new PiQValidationException("Message validation failed", validationResult.Errors);
                    exception.SetCorrelationId(context.CorrelationId);
                    throw exception;
                }
            }
            catch (PiQException ex)
            {
                ex.SetCorrelationId(context.CorrelationId);
                throw;
            }
        }
    }
}
