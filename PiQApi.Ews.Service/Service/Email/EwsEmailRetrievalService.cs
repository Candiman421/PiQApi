// PiQApi.Ews.Service/Service/Email/EwsEmailRetrievalService.cs
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
    /// Implementation of email retrieval service for Exchange
    /// </summary>
    public class EwsEmailRetrievalService : EwsServiceBase, IEwsEmailRetrievalService
    {
        private readonly IEwsPropertySetService _propertySetService;
        private readonly IEwsValidationContextFactory _validationContextFactory;
        private readonly ICertValidationProcessor _validationProcessor;
        private readonly ILogger<EwsEmailRetrievalService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsEmailRetrievalService"/> class
        /// </summary>
        public EwsEmailRetrievalService(
            IExchangeServiceWrapper serviceWrapper,
            IEwsErrorMappingService errorMappingService,
            IEwsPolicyExecutor policyExecutor,
            ICertExceptionFactory exceptionFactory,
            IEwsPropertySetService propertySetService,
            IEwsValidationContextFactory validationContextFactory,
            ICertValidationProcessor validationProcessor,
            ILogger<EwsEmailRetrievalService> logger)
            : base(serviceWrapper, errorMappingService, policyExecutor, exceptionFactory, logger)
        {
            _propertySetService = propertySetService ?? throw new ArgumentNullException(nameof(propertySetService));
            _validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
            _validationProcessor = validationProcessor ?? throw new ArgumentNullException(nameof(validationProcessor));
            _logger = logger;
        }

        /// <summary>
        /// Gets a message by ID
        /// </summary>
        public async Task<EmailMessage> GetMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(messageId);

            _logger.LogDebug("Getting message by ID {MessageId}. CorrelationId: {CorrelationId}",
                messageId.UniqueId, context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate message ID
                await ValidateItemIdAsync(messageId, context, cancellationToken).ConfigureAwait(false);

                // First operation: Get property set if needed
                PropertySet effectivePropertySet;
                if (propertySet == null)
                {
                    effectivePropertySet = await PolicyExecutor.ExecuteAsync(
                        async () => await _propertySetService.WithEmailPropertiesAsync(
                            new PropertySet(BasePropertySet.FirstClassProperties),
                            cancellationToken).ConfigureAwait(false),
                        EwsPolicyType.Service,
                        nameof(GetMessageAsync) + "_GetPropertySet",
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Validate provided property set
                    await ValidatePropertySetAsync(propertySet, context, cancellationToken).ConfigureAwait(false);
                    effectivePropertySet = propertySet;
                }

                // Second operation: Bind to message
                var message = await ExecuteAsync(
                    context,
                    async service => await EmailMessage.Bind(service, messageId, effectivePropertySet).ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(GetMessageAsync) + "_Bind",
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("GetMessage", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsRetrieved");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return message;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("GetMessage", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Gets a message with attachments
        /// </summary>
        public async Task<EmailMessage> GetMessageWithAttachmentsAsync(
            IEwsOperationContext context,
            ItemId messageId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(messageId);

            _logger.LogDebug("Getting message with attachments by ID {MessageId}. CorrelationId: {CorrelationId}",
                messageId.UniqueId, context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate message ID
                await ValidateItemIdAsync(messageId, context, cancellationToken).ConfigureAwait(false);

                // First operation: Create a property set for attachments
                var propertySet = await PolicyExecutor.ExecuteAsync(
                    () =>
                    {
                        var ps = new PropertySet(BasePropertySet.FirstClassProperties)
                        {
                            RequestedBodyType = BodyType.HTML
                        };
                        ps.Add(ItemSchema.Attachments);
                        return Task.FromResult(ps);
                    },
                    EwsPolicyType.Service,
                    nameof(GetMessageWithAttachmentsAsync) + "_CreatePropertySet",
                    cancellationToken).ConfigureAwait(false);

                // Second operation: Bind to message with attachments
                var message = await ExecuteAsync(
                    context,
                    async service => await EmailMessage.Bind(service, messageId, propertySet).ConfigureAwait(false),
                    EwsPolicyType.Mail,
                    nameof(GetMessageWithAttachmentsAsync) + "_Bind",
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("GetMessageWithAttachments", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailsWithAttachmentsRetrieved");

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return message;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("GetMessageWithAttachments", timer.Elapsed, false);

                // Ensure we log operation error
                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Finds messages in a folder that match the specified filter
        /// </summary>
        public async Task<FindItemsResults<Item>> FindMessagesAsync(
            IEwsOperationContext context,
            WellKnownFolderName folderName,
            SearchFilter? searchFilter = null,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            _logger.LogDebug("Finding messages in folder {FolderName}. CorrelationId: {CorrelationId}",
                folderName, context.CorrelationId);

            // Track operation start
            await context.LogOperationStartAsync().ConfigureAwait(false);
            var timer = Stopwatch.StartNew();

            try
            {
                // Validate search filter if provided
                if (searchFilter != null)
                {
                    await ValidateSearchFilterAsync(searchFilter, context, cancellationToken).ConfigureAwait(false);
                }

                // Validate page size
                if (pageSize <= 0 || pageSize > 1000)
                {
                    _logger.LogWarning("Invalid page size: {PageSize}. CorrelationId: {CorrelationId}",
                        pageSize, context.CorrelationId);

                    var exception = new CertValidationException("Page size must be between 1 and 1000");
                    exception.SetCorrelationId(context.CorrelationId);
                    throw exception;
                }

                // Create view for finding items
                var view = await PolicyExecutor.ExecuteAsync(
                    () =>
                    {
                        var itemView = new ItemView(pageSize)
                        {
                            PropertySet = new PropertySet(BasePropertySet.IdOnly, ItemSchema.Subject, ItemSchema.DateTimeReceived)
                        };
                        return Task.FromResult(itemView);
                    },
                    EwsPolicyType.Service,
                    nameof(FindMessagesAsync) + "_CreateView",
                    cancellationToken).ConfigureAwait(false);

                // Execute the find operation
                var results = await ExecuteAsync(
                    context,
                    async service =>
                    {
                        if (searchFilter != null)
                        {
                            return await service.FindItems(folderName, searchFilter, view).ConfigureAwait(false);
                        }
                        else
                        {
                            return await service.FindItems(folderName, view).ConfigureAwait(false);
                        }
                    },
                    EwsPolicyType.Mail,
                    nameof(FindMessagesAsync) + "_Find",
                    cancellationToken).ConfigureAwait(false);

                // Record metrics
                timer.Stop();
                context.Metrics.RecordOperation("FindMessages", timer.Elapsed, true);
                context.Metrics.IncrementCounter("EmailSearches");
                context.Metrics.IncrementCounter("EmailsFound", results.TotalCount);

                // Log successful completion
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return results;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("FindMessages", timer.Elapsed, false);

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

                    var exception = new CertValidationException("Property set validation failed", validationResult.Errors);
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
        /// Validates a search filter
        /// </summary>
        private async Task ValidateSearchFilterAsync(SearchFilter searchFilter, IEwsOperationContext context, CancellationToken cancellationToken)
        {
            try
            {
                // Create validation context with Exchange server version
                var validationContext = _validationContextFactory.CreateWithScope(
                    ServiceWrapper.Service.RequestedServerVersion,
                    "SearchFilter",
                    cancellationToken);

                // Add correlation ID for tracking
                validationContext = validationContext.WithValue("CorrelationId", context.CorrelationId);

                // Execute validation using policy to handle transient failures
                var validationResult = await PolicyExecutor.ExecuteAsync(
                    async () => await _validationProcessor.ValidateAsync(
                        searchFilter,
                        validationContext,
                        cancellationToken).ConfigureAwait(false),
                    EwsPolicyType.Validation,
                    nameof(ValidateSearchFilterAsync),
                    cancellationToken).ConfigureAwait(false);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Search filter validation failed. CorrelationId: {CorrelationId}, Errors: {ErrorCount}",
                        context.CorrelationId, validationResult.Errors.Count);

                    var exception = new CertValidationException("Search filter validation failed", validationResult.Errors);
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
