// PiQApi.Ews.Service/Service/Email/EwsEmailManagementService.cs
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
    /// Implementation of email management service for Exchange
    /// </summary>
    public class EwsEmailManagementService : EwsServiceBase, IEwsEmailManagementService
    {
        private readonly IEwsValidationContextFactory _validationContextFactory;
        private readonly ICertValidationProcessor _validationProcessor;
        private readonly ILogger<EwsEmailManagementService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsEmailManagementService"/> class
        /// </summary>
        public EwsEmailManagementService(
            IExchangeServiceWrapper serviceWrapper,
            IEwsErrorMappingService errorMappingService,
            IEwsPolicyExecutor policyExecutor,
            ICertExceptionFactory exceptionFactory,
            IEwsValidationContextFactory validationContextFactory,
            ICertValidationProcessor validationProcessor,
            ILogger<EwsEmailManagementService> logger)
            : base(serviceWrapper, errorMappingService, policyExecutor, exceptionFactory, logger)
        {
            _validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
            _validationProcessor = validationProcessor ?? throw new ArgumentNullException(nameof(validationProcessor));
            _logger = logger;
        }

        /// <summary>
        /// Updates a message
        /// </summary>
        public async Task<EmailMessage> UpdateMessageAsync(
            IEwsOperationContext context,
            EmailMessage message,
            ConflictResolutionMode resolutionMode = ConflictResolutionMode.AutoResolve,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(message);

            _logger.LogDebug("Updating message {MessageId}. CorrelationId: {CorrelationId}",
                message.Id?.UniqueId ?? "unknown", context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate message before update
                await ValidateMessageAsync(message, context, cancellationToken).ConfigureAwait(false);

                // Update the message
                await ExecuteAsync(
                    context,
                    async service => await message.Update(resolutionMode).ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(UpdateMessageAsync),
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("UpdateMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsUpdated");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return message;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("UpdateMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        public async Task DeleteMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            DeleteMode deleteMode = DeleteMode.SoftDelete,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(messageId);

            _logger.LogDebug("Deleting message {MessageId}. CorrelationId: {CorrelationId}",
                messageId.UniqueId, context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate item ID
                await ValidateItemIdAsync(messageId, context, cancellationToken).ConfigureAwait(false);

                // Delete the message
                await ExecuteAsync(
                    context,
                    async service =>
                    {
                        await service.DeleteItems(
                            new[] { messageId },
                            deleteMode,
                            SendCancellationsMode.SendToNone,
                            AffectedTaskOccurrence.AllOccurrences).ConfigureAwait(false);
                    },
                    EwsPolicyType.Mail,
                    nameof(DeleteMessageAsync),
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("DeleteMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsDeleted");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("DeleteMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Moves a message to another folder
        /// </summary>
        public async Task<EmailMessage> MoveMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            FolderId destinationFolderId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(messageId);
            ArgumentNullException.ThrowIfNull(destinationFolderId);

            _logger.LogDebug("Moving message {MessageId} to folder {FolderId}. CorrelationId: {CorrelationId}",
                messageId.UniqueId, destinationFolderId.UniqueId, context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate item ID and folder ID
                await ValidateItemIdAsync(messageId, context, cancellationToken).ConfigureAwait(false);
                await ValidateFolderIdAsync(destinationFolderId, context, cancellationToken).ConfigureAwait(false);

                // Move the message
                var result = await ExecuteAsync(
                    context,
                    async service =>
                    {
                        var moveResults = await service.MoveItems(
                            new[] { messageId },
                            destinationFolderId).ConfigureAwait(false);

                        if (moveResults.OverallResult == ServiceResult.Success)
                        {
                            // Get the first result item ID (should be only one)
                            var newItemId = moveResults[0].ItemId;

                            // Load the moved message
                            return await EmailMessage.Bind(service, newItemId).ConfigureAwait(false);
                        }
                        else
                        {
                            throw new ServiceResponseException("Failed to move message: " + moveResults[0].ErrorMessage);
                        }
                    },
                    EwsPolicyType.Mail,
                    nameof(MoveMessageAsync),
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("MoveMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsMoved");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("MoveMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

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
                    "MessageUpdate",
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

                    var exception = new CertValidationException("Message validation failed", validationResult.Errors);
                    exception.SetCorrelationId(context.CorrelationId);
                    throw exception;
                }
            }
            catch (CertException ex)
            {
                ex.SetCorrelationId(context.CorrelationId);
                throw;
            }
        }

        /// <summary>
        /// Validates an item ID
        /// </summary>
        private async Task ValidateItemIdAsync(ItemId itemId, IEwsOperationContext context, CancellationToken cancellationToken)
        {
            try
            {
                // Create validation context with Exchange server version
                var validationContext = _validationContextFactory.CreateWithScope(
                    ServiceWrapper.Service.RequestedServerVersion,
                    "ItemId",
                    cancellationToken);

                // Add correlation ID for tracking
                validationContext = validationContext.WithValue("CorrelationId", context.CorrelationId);

                // Execute validation using policy to handle transient failures
                var validationResult = await PolicyExecutor.ExecuteAsync(
                    async () => await _validationProcessor.ValidateAsync(
                        itemId,
                        validationContext,
                        cancellationToken).ConfigureAwait(false),
                    EwsPolicyType.Validation,
                    nameof(ValidateItemIdAsync),
                    cancellationToken).ConfigureAwait(false);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Item ID validation failed. CorrelationId: {CorrelationId}, Errors: {ErrorCount}",
                        context.CorrelationId, validationResult.Errors.Count);

                    var exception = new CertValidationException("Item ID validation failed", validationResult.Errors);
                    exception.SetCorrelationId(context.CorrelationId);
                    throw exception;
                }
            }
            catch (CertException ex)
            {
                ex.SetCorrelationId(context.CorrelationId);
                throw;
            }
        }

        /// <summary>
        /// Validates a folder ID
        /// </summary>
        private async Task ValidateFolderIdAsync(FolderId folderId, IEwsOperationContext context, CancellationToken cancellationToken)
        {
            try
            {
                // Create validation context with Exchange server version
                var validationContext = _validationContextFactory.CreateWithScope(
                    ServiceWrapper.Service.RequestedServerVersion,
                    "FolderId",
                    cancellationToken);

                // Add correlation ID for tracking
                validationContext = validationContext.WithValue("CorrelationId", context.CorrelationId);

                // Execute validation using policy to handle transient failures
                var validationResult = await PolicyExecutor.ExecuteAsync(
                    async () => await _validationProcessor.ValidateAsync(
                        folderId,
                        validationContext,
                        cancellationToken).ConfigureAwait(false),
                    EwsPolicyType.Validation,
                    nameof(ValidateFolderIdAsync),
                    cancellationToken).ConfigureAwait(false);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Folder ID validation failed. CorrelationId: {CorrelationId}, Errors: {ErrorCount}",
                        context.CorrelationId, validationResult.Errors.Count);

                    var exception = new CertValidationException("Folder ID validation failed", validationResult.Errors);
                    exception.SetCorrelationId(context.CorrelationId);
                    throw exception;
                }
            }
            catch (CertException ex)
            {
                ex.SetCorrelationId(context.CorrelationId);
                throw;
            }
        }
    }
}
