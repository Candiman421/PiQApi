// PiQApi.Ews.Operations/Mail/Operations/EwsMailOperations.cs

using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Core.Results.Interfaces;
using PiQApi.Ews.Operations.Core.Base;
using PiQApi.Ews.Operations.Core.Interfaces;
using PiQApi.Ews.Service.Core.Interfaces;
using PiQApi.Ews.Service.Service.Email;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Ews.Operations.Mail.Operations
{
    /// <summary>
    /// Implementation of mail operations for Exchange
    /// </summary>
    public class EwsMailOperations : EwsOperationBase, IEwsMailOperations
    {
        private readonly IEwsEmailCreateService _emailCreateService;
        private readonly IEwsEmailManagementService _emailManagementService;
        private readonly IEwsEmailRetrievalService _emailRetrievalService;
        private readonly IEwsEmailCommunicationService _emailCommunicationService;
        private readonly IEwsPropertySetService _propertySetService;
        private readonly IEwsResultFactory _resultFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsMailOperations"/> class
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="serviceWrapper">Exchange service wrapper</param>
        /// <param name="emailCreateService">Email creation service</param>
        /// <param name="emailManagementService">Email management service</param>
        /// <param name="emailRetrievalService">Email retrieval service</param>
        /// <param name="emailCommunicationService">Email communication service</param>
        /// <param name="propertySetService">Property set service</param>
        /// <param name="resultFactory">Result factory</param>
        /// <param name="logger">Logger</param>
        /// <param name="exceptionFactory">Exception factory</param>
        /// <param name="validationProcessor">Validation processor</param>
        public EwsMailOperations(
            EwsOperationContext context,
            IExchangeServiceWrapper serviceWrapper,
            IEwsEmailCreateService emailCreateService,
            IEwsEmailManagementService emailManagementService,
            IEwsEmailRetrievalService emailRetrievalService,
            IEwsEmailCommunicationService emailCommunicationService,
            IEwsPropertySetService propertySetService,
            IEwsResultFactory resultFactory,
            ILogger<EwsMailOperations> logger,
            IPiQExceptionFactory exceptionFactory,
            IPiQValidationProcessor validationProcessor)
            : base(context, serviceWrapper, logger, exceptionFactory, validationProcessor)
        {
            _emailCreateService = emailCreateService ?? throw new ArgumentNullException(nameof(emailCreateService));
            _emailManagementService = emailManagementService ?? throw new ArgumentNullException(nameof(emailManagementService));
            _emailRetrievalService = emailRetrievalService ?? throw new ArgumentNullException(nameof(emailRetrievalService));
            _emailCommunicationService = emailCommunicationService ?? throw new ArgumentNullException(nameof(emailCommunicationService));
            _propertySetService = propertySetService ?? throw new ArgumentNullException(nameof(propertySetService));
            _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        }

        /// <summary>
        /// Creates a new email message
        /// </summary>
        public async Task<IEwsResult<EmailMessage>> CreateEmailAsync(
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await ValidateStateAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                Logger.LogInformation("Creating new email message. CorrelationId: {CorrelationId}", CorrelationId);

                EmailMessage message;
                if (propertySet != null)
                {
                    message = await _emailCreateService.CreateMessageWithPropertySetAsync(
                        Context,
                        propertySet,
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    message = await _emailCreateService.CreateMessageAsync(
                        Context,
                        cancellationToken).ConfigureAwait(false);
                }

                return _resultFactory.Success(message, Context.CorrelationId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating email message. CorrelationId: {CorrelationId}", CorrelationId);

                if (ex is PiQException piqException)
                {
                    return _resultFactory.Failure<EmailMessage>(piqException, Context.CorrelationId);
                }

                var wrappedException = CreateExceptionWithCorrelation(
                    "Failed to create email message",
                    "EmailCreationFailed",
                    nameof(CreateEmailAsync));

                return _resultFactory.Failure<EmailMessage>(wrappedException, Context.CorrelationId);
            }
        }

        /// <summary>
        /// Creates a backdated email message with the specified date
        /// </summary>
        public async Task<IEwsResult<EmailMessage>> CreateBackdatedEmailAsync(
            DateTime clientSubmitTime,
            DateTime receivedTime,
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await ValidateStateAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                Logger.LogInformation(
                    "Creating backdated email message. ClientSubmitTime: {ClientSubmitTime}, ReceivedTime: {ReceivedTime}, CorrelationId: {CorrelationId}",
                    clientSubmitTime, receivedTime, CorrelationId);

                // Create base email first
                var createResult = await CreateEmailAsync(propertySet, cancellationToken).ConfigureAwait(false);
                if (!createResult.IsSuccess)
                {
                    return createResult;
                }

                var message = createResult.Value;

                // Define extended properties for backdating
                var clientSubmitTimeProperty = new ExtendedPropertyDefinition(
                    DefaultExtendedPropertySet.Common,
                    0x0039, // PidTagClientSubmitTime
                    MapiPropertyType.SystemTime);

                var receivedTimeProperty = new ExtendedPropertyDefinition(
                    DefaultExtendedPropertySet.Common,
                    0x0E06, // PidTagMessageDeliveryTime
                    MapiPropertyType.SystemTime);

                // Set the extended properties
                message.SetExtendedProperty(clientSubmitTimeProperty, clientSubmitTime);
                message.SetExtendedProperty(receivedTimeProperty, receivedTime);

                // Return the backdated message
                return _resultFactory.Success(message, Context.CorrelationId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating backdated email message. CorrelationId: {CorrelationId}", CorrelationId);

                if (ex is PiQException piqException)
                {
                    return _resultFactory.Failure<EmailMessage>(piqException, Context.CorrelationId);
                }

                var wrappedException = CreateExceptionWithCorrelation(
                    "Failed to create backdated email message",
                    "BackdatedEmailCreationFailed",
                    nameof(CreateBackdatedEmailAsync));

                return _resultFactory.Failure<EmailMessage>(wrappedException, Context.CorrelationId);
            }
        }

        /// <summary>
        /// Saves an email message as a draft
        /// </summary>
        public async Task<IEwsResult> SaveDraftAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await ValidateStateAsync(cancellationToken).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(message);

            try
            {
                Logger.LogInformation("Saving email message as draft. CorrelationId: {CorrelationId}", CorrelationId);

                await _emailCreateService.SaveDraftAsync(
                    Context,
                    message,
                    cancellationToken).ConfigureAwait(false);

                return _resultFactory.Success(Context.CorrelationId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving email message as draft. CorrelationId: {CorrelationId}", CorrelationId);

                if (ex is PiQException piqException)
                {
                    return _resultFactory.Failure(piqException, Context.CorrelationId);
                }

                var wrappedException = CreateExceptionWithCorrelation(
                    "Failed to save email message as draft",
                    "SaveDraftFailed",
                    nameof(SaveDraftAsync));

                return _resultFactory.Failure(wrappedException, Context.CorrelationId);
            }
        }

        /// <summary>
        /// Sends an email message
        /// </summary>
        public async Task<IEwsResult> SendEmailAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await ValidateStateAsync(cancellationToken).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(message);

            try
            {
                Logger.LogInformation("Sending email message. CorrelationId: {CorrelationId}", CorrelationId);

                await _emailCommunicationService.SendMessageAsync(
                    Context,
                    message,
                    cancellationToken).ConfigureAwait(false);

                return _resultFactory.Success(Context.CorrelationId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending email message. CorrelationId: {CorrelationId}", CorrelationId);

                if (ex is PiQException piqException)
                {
                    return _resultFactory.Failure(piqException, Context.CorrelationId);
                }

                var wrappedException = CreateExceptionWithCorrelation(
                    "Failed to send email message",
                    "SendEmailFailed",
                    nameof(SendEmailAsync));

                return _resultFactory.Failure(wrappedException, Context.CorrelationId);
            }
        }

        /// <summary>
        /// Gets an email message by ID
        /// </summary>
        public async Task<IEwsResult<EmailMessage>> GetEmailAsync(
            ItemId messageId,
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await ValidateStateAsync(cancellationToken).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(messageId);

            try
            {
                Logger.LogInformation("Getting email message by ID {MessageId}. CorrelationId: {CorrelationId}",
                    messageId.UniqueId, CorrelationId);

                var message = await _emailRetrievalService.GetMessageAsync(
                    Context,
                    messageId,
                    propertySet,
                    cancellationToken).ConfigureAwait(false);

                return _resultFactory.Success(message, Context.CorrelationId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting email message. CorrelationId: {CorrelationId}", CorrelationId);

                if (ex is PiQException piqException)
                {
                    return _resultFactory.Failure<EmailMessage>(piqException, Context.CorrelationId);
                }

                var wrappedException = CreateExceptionWithCorrelation(
                    "Failed to get email message",
                    "GetEmailFailed",
                    nameof(GetEmailAsync));

                return _resultFactory.Failure<EmailMessage>(wrappedException, Context.CorrelationId);
            }
        }

        /// <summary>
        /// Called during state validation
        /// </summary>
        protected override async Task OnValidateStateAsync(CancellationToken cancellationToken)
        {
            await base.OnValidateStateAsync(cancellationToken).ConfigureAwait(false);

            // Additional validation specific to mail operations
            if (ServiceWrapper.AuthenticationStatus != AuthenticationStatusType.Authenticated)
            {
                throw CreateExceptionWithCorrelation(
                    "Exchange service is not authenticated",
                    "NotAuthenticated",
                    nameof(OnValidateStateAsync));
            }
        }
    }
}
