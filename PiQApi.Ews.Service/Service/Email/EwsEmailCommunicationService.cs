// PiQApi.Ews.Service/Service/Email/EwsEmailCommunicationService.cs
using System.Diagnostics;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
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
    /// Implementation of email communication service for Exchange
    /// </summary>
    public class EwsEmailCommunicationService : EwsServiceBase, IEwsEmailCommunicationService
    {
        private readonly IEwsValidationContextFactory _validationContextFactory;
        private readonly ICertValidationProcessor _validationProcessor;
        private readonly ILogger<EwsEmailCommunicationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsEmailCommunicationService"/> class
        /// </summary>
        public EwsEmailCommunicationService(
            IExchangeServiceWrapper serviceWrapper,
            IEwsErrorMappingService errorMappingService,
            IEwsPolicyExecutor policyExecutor,
            ICertExceptionFactory exceptionFactory,
            IEwsValidationContextFactory validationContextFactory,
            ICertValidationProcessor validationProcessor,
            ILogger<EwsEmailCommunicationService> logger)
            : base(serviceWrapper, errorMappingService, policyExecutor, exceptionFactory, logger)
        {
            _validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
            _validationProcessor = validationProcessor ?? throw new ArgumentNullException(nameof(validationProcessor));
            _logger = logger;
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        public async Task SendMessageAsync(
            IEwsOperationContext context,
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(message);

            _logger.LogDebug("Sending message. CorrelationId: {CorrelationId}", context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Create validation context for message
                var validationContext = CreateValidationContext(context, "MessageSend", cancellationToken);

                // Validate message using processor with registered rules
                var validationResult = await PolicyExecutor.ExecuteAsync(
                    async () => await _validationProcessor.ValidateAsync(message, validationContext, cancellationToken).ConfigureAwait(false),
                    EwsPolicyType.Validation,
                    nameof(SendMessageAsync) + "_ValidateMessage",
                    cancellationToken).ConfigureAwait(false);

                HandleValidationResult(validationResult, "message", context.CorrelationId);

                // Validate recipients if present
                if (message.ToRecipients?.Count > 0)
                {
                    await ValidateEntity(message.ToRecipients, "EmailAddresses", context, cancellationToken);
                }

                if (message.CcRecipients?.Count > 0)
                {
                    await ValidateEntity(message.CcRecipients, "EmailAddresses", context, cancellationToken);
                }

                if (message.BccRecipients?.Count > 0)
                {
                    await ValidateEntity(message.BccRecipients, "EmailAddresses", context, cancellationToken);
                }

                // Send the message
                await ExecuteAsync(
                    context,
                    async service => await message.Send().ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(SendMessageAsync),
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("SendMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsSent");

                // Count recipients for metrics
                int recipientCount = 0;
                recipientCount += message.ToRecipients?.Count ?? 0;
                recipientCount += message.CcRecipients?.Count ?? 0;
                recipientCount += message.BccRecipients?.Count ?? 0;

                if (recipientCount > 0)
                {
                    context.Metrics.IncrementCounter("EmailRecipients", recipientCount);
                }

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("SendMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Forwards a message to recipients
        /// </summary>
        public async Task<EmailMessage> ForwardMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            IEnumerable<string> toRecipients,
            string comment,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(messageId);
            ArgumentNullException.ThrowIfNull(toRecipients);

            var recipientList = toRecipients.ToList();
            if (recipientList.Count == 0)
            {
                throw new ArgumentException("At least one recipient is required", nameof(toRecipients));
            }

            _logger.LogDebug("Forwarding message {MessageId} to {RecipientCount} recipients. CorrelationId: {CorrelationId}",
                messageId.UniqueId, recipientList.Count, context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate item ID
                await ValidateEntity(messageId, "ItemId", context, cancellationToken);

                // First operation: Get the message
                var message = await ExecuteAsync(
                    context,
                    async service => await EmailMessage.Bind(service, messageId, PropertySet.FirstClassProperties).ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(ForwardMessageAsync) + "_Bind",
                    cancellationToken).ConfigureAwait(false);

                // Second operation: Create forward response
                var forward = await PolicyExecutor.ExecuteAsync(
                    () =>
                    {
                        var fwd = message.CreateForward();
                        fwd.Body = new MessageBody(comment ?? string.Empty);
                        return Task.FromResult(fwd);
                    },
                    EwsPolicyType.Mail,
                    nameof(ForwardMessageAsync) + "_CreateForward",
                    cancellationToken).ConfigureAwait(false);

                // Third operation: Add recipients
                await PolicyExecutor.ExecuteAsync(
                    () =>
                    {
                        foreach (var recipient in recipientList)
                        {
                            forward.ToRecipients.Add(recipient);
                        }
                        return Task.CompletedTask;
                    },
                    EwsPolicyType.Mail,
                    nameof(ForwardMessageAsync) + "_AddRecipients",
                    cancellationToken).ConfigureAwait(false);

                // Validate recipients
                await ValidateEntity(forward.ToRecipients, "EmailAddresses", context, cancellationToken);

                // Fourth operation: Send the forward
                await ExecuteAsync(
                    context,
                    async service => await forward.SendAndSaveCopy().ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(ForwardMessageAsync) + "_Send",
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("ForwardMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsForwarded");
                context.Metrics.IncrementCounter("EmailRecipients", recipientList.Count);

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return message;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("ForwardMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Replies to a message
        /// </summary>
        public async Task<EmailMessage> ReplyToMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            string response,
            bool replyAll = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(messageId);
            ArgumentNullException.ThrowIfNull(response);

            _logger.LogDebug("Replying to message {MessageId}, reply all: {ReplyAll}. CorrelationId: {CorrelationId}",
                messageId.UniqueId, replyAll, context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate item ID
                await ValidateEntity(messageId, "ItemId", context, cancellationToken);

                // First operation: Get the message
                var message = await ExecuteAsync(
                    context,
                    async service => await EmailMessage.Bind(service, messageId, PropertySet.FirstClassProperties).ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(ReplyToMessageAsync) + "_Bind",
                    cancellationToken).ConfigureAwait(false);

                // Second operation: Create reply
                var reply = await PolicyExecutor.ExecuteAsync(
                    () =>
                    {
                        var rply = message.CreateReply(replyAll);
                        rply.Body = new MessageBody(response);
                        return Task.FromResult(rply);
                    },
                    EwsPolicyType.Mail,
                    nameof(ReplyToMessageAsync) + "_CreateReply",
                    cancellationToken).ConfigureAwait(false);

                // Validate body
                await ValidateEntity(reply.Body, "MessageBody", context, cancellationToken);

                // Third operation: Send the reply
                await ExecuteAsync(
                    context,
                    async service => await reply.SendAndSaveCopy().ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(ReplyToMessageAsync) + "_Send",
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("ReplyToMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsReplied");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return message;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("ReplyToMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Creates a validation context for the specified scope
        /// </summary>
        private CertValidationContext CreateValidationContext(
            IEwsOperationContext context,
            string scope,
            CancellationToken cancellationToken)
        {
            var validationContext = _validationContextFactory.CreateWithScope(
                ServiceWrapper.Service.RequestedServerVersion,
                scope,
                cancellationToken);

            return validationContext.WithValue("CorrelationId", context.CorrelationId);
        }

        /// <summary>
        /// Validates an entity using the validation processor with registered rules
        /// </summary>
        private async Task ValidateEntity<T>(
            T entity,
            string scope,
            IEwsOperationContext context,
            CancellationToken cancellationToken) where T : class
        {
            var validationContext = CreateValidationContext(context, scope, cancellationToken);

            var validationResult = await PolicyExecutor.ExecuteAsync(
                async () => await _validationProcessor.ValidateAsync(
                    entity,
                    validationContext,
                    cancellationToken).ConfigureAwait(false),
                EwsPolicyType.Validation,
                $"Validate{typeof(T).Name}",
                cancellationToken).ConfigureAwait(false);

            HandleValidationResult(validationResult, typeof(T).Name, context.CorrelationId);
        }

        /// <summary>
        /// Handles validation result by throwing appropriate exception if validation failed
        /// </summary>
        private void HandleValidationResult(
            CertValidationResult validationResult,
            string entityType,
            string correlationId)
        {
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("{EntityType} validation failed. CorrelationId: {CorrelationId}, Errors: {ErrorCount}",
                    entityType, correlationId, validationResult.Errors.Count);

                var exception = new CertValidationException(
                    $"{entityType} validation failed",
                    validationResult.Errors);

                exception.SetCorrelationId(correlationId);
                throw exception;
            }
        }
    }
}
