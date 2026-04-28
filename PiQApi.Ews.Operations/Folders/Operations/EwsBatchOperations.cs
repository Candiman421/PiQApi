// PiQApi.Ews.Operations/Folders/Operations/EwsBatchOperations.cs

using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Core.Results.Interfaces;
using PiQApi.Ews.Operations.Core.Base;
using PiQApi.Ews.Operations.Folders.Interfaces;
using PiQApi.Ews.Operations.Folders.Models;
using PiQApi.Ews.Service.Core.Interfaces;
using PiQApi.Ews.Service.Service.Email;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Ews.Operations.Folders.Operations
{
    /// <summary>
    /// Implementation of batch operations for Exchange
    /// </summary>
    public class EwsBatchOperations : EwsOperationBase, IEwsBatchOperations
    {
        private readonly IEwsEmailCreateService _emailCreateService;
        private readonly IEwsEmailManagementService _emailManagementService;
        private readonly IEwsPropertySetService _propertySetService;
        private readonly IEwsResultFactory _resultFactory;

        // Maximum items per batch based on EWS API limits
        private const int MaxBatchSize = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsBatchOperations"/> class
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="serviceWrapper">Exchange service wrapper</param>
        /// <param name="emailCreateService">Email creation service</param>
        /// <param name="emailManagementService">Email management service</param>
        /// <param name="propertySetService">Property set service</param>
        /// <param name="resultFactory">Result factory</param>
        /// <param name="logger">Logger</param>
        /// <param name="exceptionFactory">Exception factory</param>
        /// <param name="validationProcessor">Validation processor</param>
        public EwsBatchOperations(
            EwsOperationContext context,
            IExchangeServiceWrapper serviceWrapper,
            IEwsEmailCreateService emailCreateService,
            IEwsEmailManagementService emailManagementService,
            IEwsPropertySetService propertySetService,
            IEwsResultFactory resultFactory,
            ILogger<EwsBatchOperations> logger,
            IPiQExceptionFactory exceptionFactory,
            IPiQValidationProcessor validationProcessor)
            : base(context, serviceWrapper, logger, exceptionFactory, validationProcessor)
        {
            _emailCreateService = emailCreateService ?? throw new ArgumentNullException(nameof(emailCreateService));
            _emailManagementService = emailManagementService ?? throw new ArgumentNullException(nameof(emailManagementService));
            _propertySetService = propertySetService ?? throw new ArgumentNullException(nameof(propertySetService));
            _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        }

        /// <summary>
        /// Imports a batch of emails into a folder
        /// </summary>
        public async Task<IEwsResult<BatchImportResult>> ImportEmailsAsync(
            IEnumerable<EmailImportInfo> emails,
            FolderId destinationFolderId,
            BatchOperationModeType operationMode = BatchOperationModeType.Parallel,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await ValidateStateAsync(cancellationToken).ConfigureAwait(false);
            
            ArgumentNullException.ThrowIfNull(emails);
            ArgumentNullException.ThrowIfNull(destinationFolderId);

            try
            {
                var emailList = emails.ToList();
                if (emailList.Count == 0)
                {
                    Logger.LogWarning("No emails provided for import. CorrelationId: {CorrelationId}", CorrelationId);
                    return _resultFactory.Success(new BatchImportResult(0, 0, new List<EmailImportError>()), Context.CorrelationId);
                }

                Logger.LogInformation("Importing {Count} emails to folder {FolderId}. Mode: {OperationMode}, CorrelationId: {CorrelationId}",
                    emailList.Count, destinationFolderId.UniqueId, operationMode, CorrelationId);

                var result = await (operationMode == BatchOperationModeType.Parallel
                    ? ImportEmailsParallelAsync(emailList, destinationFolderId, cancellationToken)
                    : ImportEmailsSequentialAsync(emailList, destinationFolderId, cancellationToken)).ConfigureAwait(false);

                Logger.LogInformation(
                    "Batch import completed. Successful: {SuccessCount}, Failed: {FailureCount}, CorrelationId: {CorrelationId}",
                    result.SuccessCount, result.FailureCount, CorrelationId);

                return _resultFactory.Success(result, Context.CorrelationId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during batch import. CorrelationId: {CorrelationId}", CorrelationId);

                if (ex is PiQException certException)
                {
                    return _resultFactory.Failure<BatchImportResult>(certException, Context.CorrelationId);
                }

                var wrappedException = CreateExceptionWithCorrelation(
                    "Failed to import emails batch",
                    "BatchImportFailed",
                    nameof(ImportEmailsAsync));

                return _resultFactory.Failure<BatchImportResult>(wrappedException, Context.CorrelationId);
            }
        }

        /// <summary>
        /// Imports emails with original metadata preserved
        /// </summary>
        public async Task<IEwsResult<BatchImportResult>> ImportEmailsWithMetadataAsync(
            IEnumerable<EmailImportInfo> emails,
            FolderId destinationFolderId,
            bool preserveOriginalDates = true,
            BatchOperationModeType operationMode = BatchOperationModeType.Parallel,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await ValidateStateAsync(cancellationToken).ConfigureAwait(false);
            
            ArgumentNullException.ThrowIfNull(emails);
            ArgumentNullException.ThrowIfNull(destinationFolderId);

            try
            {
                var emailList = emails.ToList();
                if (emailList.Count == 0)
                {
                    Logger.LogWarning("No emails provided for metadata import. CorrelationId: {CorrelationId}", CorrelationId);
                    return _resultFactory.Success(new BatchImportResult(0, 0, new List<EmailImportError>()), Context.CorrelationId);
                }

                Logger.LogInformation(
                    "Importing {Count} emails with metadata to folder {FolderId}. PreserveOriginalDates: {PreserveOriginalDates}, Mode: {OperationMode}, CorrelationId: {CorrelationId}",
                    emailList.Count, destinationFolderId.UniqueId, preserveOriginalDates, operationMode, CorrelationId);

                // Create property set with extended properties for dates
                var propertySet = await _propertySetService.WithEmailPropertiesAsync(
                    new PropertySet(BasePropertySet.FirstClassProperties), 
                    cancellationToken).ConfigureAwait(false);

                var result = new BatchImportResult();
                var errors = new List<EmailImportError>();
                int successCount = 0;

                // Process in batches to avoid excessive parallel operations
                foreach (var batch in CreateBatches(emailList, MaxBatchSize))
                {
                    // Process each batch with the specified operation mode
                    var batchResults = await (operationMode == BatchOperationModeType.Parallel
                        ? ImportBatchWithMetadataParallelAsync(batch, destinationFolderId, propertySet, preserveOriginalDates, cancellationToken)
                        : ImportBatchWithMetadataSequentialAsync(batch, destinationFolderId, propertySet, preserveOriginalDates, cancellationToken)).ConfigureAwait(false);

                    // Combine results
                    successCount += batchResults.SuccessCount;
                    errors.AddRange(batchResults.Errors);

                    // Check for cancellation between batches
                    cancellationToken.ThrowIfCancellationRequested();
                }

                var finalResult = new BatchImportResult(successCount, errors.Count, errors);

                Logger.LogInformation(
                    "Batch metadata import completed. Successful: {SuccessCount}, Failed: {FailureCount}, CorrelationId: {CorrelationId}",
                    finalResult.SuccessCount, finalResult.FailureCount, CorrelationId);

                return _resultFactory.Success(finalResult, Context.CorrelationId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during batch metadata import. CorrelationId: {CorrelationId}", CorrelationId);

                if (ex is PiQException certException)
                {
                    return _resultFactory.Failure<BatchImportResult>(certException, Context.CorrelationId);
                }

                var wrappedException = CreateExceptionWithCorrelation(
                    "Failed to import emails with metadata",
                    "BatchMetadataImportFailed",
                    nameof(ImportEmailsWithMetadataAsync));

                return _resultFactory.Failure<BatchImportResult>(wrappedException, Context.CorrelationId);
            }
        }

        /// <summary>
        /// Imports emails in parallel
        /// </summary>
        private async Task<BatchImportResult> ImportEmailsParallelAsync(
            List<EmailImportInfo> emails,
            FolderId destinationFolderId,
            CancellationToken cancellationToken)
        {
            var errors = new List<EmailImportError>();
            var tasks = new List<Task<(bool Success, EmailImportError? Error)>>();

            // Process in batches to avoid excessive parallel operations
            foreach (var batch in CreateBatches(emails, MaxBatchSize))
            {
                // Start a task for each email in the batch
                foreach (var email in batch)
                {
                    tasks.Add(ImportSingleEmailAsync(email, destinationFolderId, cancellationToken));
                }

                // Wait for all tasks in this batch to complete
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                
                // Collect errors
                errors.AddRange(results.Where(r => !r.Success && r.Error != null).Select(r => r.Error!));
                
                // Clear tasks for next batch
                tasks.Clear();

                // Check for cancellation between batches
                cancellationToken.ThrowIfCancellationRequested();
            }

            int successCount = emails.Count - errors.Count;
            return new BatchImportResult(successCount, errors.Count, errors);
        }

        /// <summary>
        /// Imports emails sequentially
        /// </summary>
        private async Task<BatchImportResult> ImportEmailsSequentialAsync(
            List<EmailImportInfo> emails,
            FolderId destinationFolderId,
            CancellationToken cancellationToken)
        {
            var errors = new List<EmailImportError>();
            int successCount = 0;

            foreach (var email in emails)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var (success, error) = await ImportSingleEmailAsync(email, destinationFolderId, cancellationToken).ConfigureAwait(false);
                
                if (success)
                {
                    successCount++;
                }
                else if (error != null)
                {
                    errors.Add(error);
                }
            }

            return new BatchImportResult(successCount, errors.Count, errors);
        }

        /// <summary>
        /// Imports a single email
        /// </summary>
        private async Task<(bool Success, EmailImportError? Error)> ImportSingleEmailAsync(
            EmailImportInfo email,
            FolderId destinationFolderId,
            CancellationToken cancellationToken)
        {
            try
            {
                // Create a new message
                var message = await _emailCreateService.CreateMessageAsync(Context, cancellationToken).ConfigureAwait(false);

                // Set message properties
                message.Subject = email.Subject;
                message.Body = new MessageBody(email.BodyType == BodyType.HTML ? BodyType.HTML : BodyType.Text, email.Body);
                
                // Set recipients
                if (email.ToRecipients != null)
                {
                    foreach (var recipient in email.ToRecipients)
                    {
                        message.ToRecipients.Add(recipient);
                    }
                }
                
                if (email.CcRecipients != null)
                {
                    foreach (var recipient in email.CcRecipients)
                    {
                        message.CcRecipients.Add(recipient);
                    }
                }
                
                if (email.BccRecipients != null)
                {
                    foreach (var recipient in email.BccRecipients)
                    {
                        message.BccRecipients.Add(recipient);
                    }
                }

                // Set importance
                message.Importance = email.Importance;

                // Save to destination folder
                await message.Save(destinationFolderId).ConfigureAwait(false);

                return (true, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error importing email {Subject}. CorrelationId: {CorrelationId}", 
                    email.Subject, CorrelationId);
                
                return (false, new EmailImportError(email.Subject, ex.Message));
            }
        }

        /// <summary>
        /// Imports a batch of emails with metadata in parallel
        /// </summary>
        private async Task<BatchImportResult> ImportBatchWithMetadataParallelAsync(
            List<EmailImportInfo> batch,
            FolderId destinationFolderId,
            PropertySet propertySet,
            bool preserveOriginalDates,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task<(bool Success, EmailImportError? Error)>>();
            
            foreach (var email in batch)
            {
                tasks.Add(ImportSingleEmailWithMetadataAsync(
                    email, 
                    destinationFolderId, 
                    propertySet, 
                    preserveOriginalDates, 
                    cancellationToken));
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            
            var errors = results
                .Where(r => !r.Success && r.Error != null)
                .Select(r => r.Error!)
                .ToList();
                
            int successCount = batch.Count - errors.Count;
            
            return new BatchImportResult(successCount, errors.Count, errors);
        }

        /// <summary>
        /// Imports a batch of emails with metadata sequentially
        /// </summary>
        private async Task<BatchImportResult> ImportBatchWithMetadataSequentialAsync(
            List<EmailImportInfo> batch,
            FolderId destinationFolderId,
            PropertySet propertySet,
            bool preserveOriginalDates,
            CancellationToken cancellationToken)
        {
            var errors = new List<EmailImportError>();
            int successCount = 0;

            foreach (var email in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var (success, error) = await ImportSingleEmailWithMetadataAsync(
                    email, 
                    destinationFolderId, 
                    propertySet, 
                    preserveOriginalDates, 
                    cancellationToken).ConfigureAwait(false);
                
                if (success)
                {
                    successCount++;
                }
                else if (error != null)
                {
                    errors.Add(error);
                }
            }

            return new BatchImportResult(successCount, errors.Count, errors);
        }

        /// <summary>
        /// Imports a single email with full metadata preservation
        /// </summary>
        private async Task<(bool Success, EmailImportError? Error)> ImportSingleEmailWithMetadataAsync(
            EmailImportInfo email,
            FolderId destinationFolderId,
            PropertySet propertySet,
            bool preserveOriginalDates,
            CancellationToken cancellationToken)
        {
            try
            {
                // Create a new message with property set
                var message = await _emailCreateService.CreateMessageWithPropertySetAsync(
                    Context, 
                    propertySet, 
                    cancellationToken).ConfigureAwait(false);

                // Set basic properties
                message.Subject = email.Subject;
                message.Body = new MessageBody(email.BodyType == BodyType.HTML ? BodyType.HTML : BodyType.Text, email.Body);
                
                // Set recipients
                if (email.ToRecipients != null)
                {
                    foreach (var recipient in email.ToRecipients)
                    {
                        message.ToRecipients.Add(recipient);
                    }
                }
                
                if (email.CcRecipients != null)
                {
                    foreach (var recipient in email.CcRecipients)
                    {
                        message.CcRecipients.Add(recipient);
                    }
                }
                
                if (email.BccRecipients != null)
                {
                    foreach (var recipient in email.BccRecipients)
                    {
                        message.BccRecipients.Add(recipient);
                    }
                }
                
                // Set sender if available
                if (!string.IsNullOrEmpty(email.FromAddress))
                {
                    message.From = new EmailAddress(email.FromAddress);
                }

                // Set importance
                message.Importance = email.Importance;

                // Set extended metadata properties
                if (preserveOriginalDates && email.OriginalSentDate.HasValue)
                {
                    // Define extended properties for dates
                    var clientSubmitTimeProperty = new ExtendedPropertyDefinition(
                        DefaultExtendedPropertySet.Common,
                        0x0039, // PidTagClientSubmitTime
                        MapiPropertyType.SystemTime);

                    var receivedTimeProperty = new ExtendedPropertyDefinition(
                        DefaultExtendedPropertySet.Common,
                        0x0E06, // PidTagMessageDeliveryTime
                        MapiPropertyType.SystemTime);

                    var sentTimeProperty = new ExtendedPropertyDefinition(
                        DefaultExtendedPropertySet.Common,
                        0x0E06, // PidTagMessageDeliveryTime same as above
                        MapiPropertyType.SystemTime);

                    // Set date properties
                    message.SetExtendedProperty(clientSubmitTimeProperty, email.OriginalSentDate.Value);
                    
                    // Set received date if available, otherwise use sent date
                    DateTime receivedDate = email.OriginalReceivedDate ?? email.OriginalSentDate.Value;
                    message.SetExtendedProperty(receivedTimeProperty, receivedDate);
                    message.SetExtendedProperty(sentTimeProperty, email.OriginalSentDate.Value);
                }

                // Set any custom extended properties
                if (email.ExtendedProperties != null)
                {
                    foreach (var prop in email.ExtendedProperties)
                    {
                        var propDef = new ExtendedPropertyDefinition(
                            DefaultExtendedPropertySet.InternetHeaders,
                            prop.Key,
                            MapiPropertyType.String);

                        message.SetExtendedProperty(propDef, prop.Value);
                    }
                }

                // Add attachments if any
                if (email.Attachments != null && email.Attachments.Any())
                {
                    foreach (var attachment in email.Attachments)
                    {
                        if (attachment.IsFileAttachment && attachment.Content != null)
                        {
                            var fileAttachment = message.Attachments.AddFileAttachment(
                                attachment.Name,
                                attachment.Content);
                                
                            if (!string.IsNullOrEmpty(attachment.ContentId))
                            {
                                fileAttachment.ContentId = attachment.ContentId;
                            }
                            
                            if (!string.IsNullOrEmpty(attachment.ContentType))
                            {
                                fileAttachment.ContentType = attachment.ContentType;
                            }
                        }
                    }
                }

                // Save to destination folder
                await message.Save(destinationFolderId).ConfigureAwait(false);

                return (true, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error importing email with metadata {Subject}. CorrelationId: {CorrelationId}", 
                    email.Subject, CorrelationId);
                
                return (false, new EmailImportError(email.Subject, ex.Message));
            }
        }

        /// <summary>
        /// Creates batches from a list of items
        /// </summary>
        private IEnumerable<List<T>> CreateBatches<T>(List<T> items, int batchSize)
        {
            for (int i = 0; i < items.Count; i += batchSize)
            {
                yield return items.Skip(i).Take(batchSize).ToList();
            }
        }

        /// <summary>
        /// Called during state validation
        /// </summary>
        protected override async Task OnValidateStateAsync(CancellationToken cancellationToken)
        {
            await base.OnValidateStateAsync(cancellationToken).ConfigureAwait(false);

            // Additional validation specific to batch operations
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