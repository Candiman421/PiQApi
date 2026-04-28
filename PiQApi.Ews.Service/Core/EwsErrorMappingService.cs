// PiQApi.Ews.Service/Core/EwsErrorMappingService.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Ews.Service.Core.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;

namespace PiQApi.Ews.Service.Core
{
    /// <summary>
    /// Service for mapping Exchange errors to application errors
    /// </summary>
    public class EwsErrorMappingService : IEwsErrorMappingService
    {
        private readonly ILogger<EwsErrorMappingService> _logger;
        private readonly IPiQExceptionFactory _exceptionFactory;

        // Dictionary mapping Exchange service errors to error messages
        private static readonly Dictionary<ServiceError, string> ServiceErrorMessages = new()
        {
            { ServiceError.ErrorAccessDenied, "Access denied to the requested resource" },
            { ServiceError.ErrorItemNotFound, "The requested item could not be found" },
            { ServiceError.ErrorFolderNotFound, "The requested folder could not be found" },
            { ServiceError.ErrorServerBusy, "The Exchange server is temporarily busy, please retry later" },
            { ServiceError.ErrorImpersonateUserDenied, "Impersonation access denied" },
            { ServiceError.ErrorInvalidOperation, "The operation is invalid in the current context" },
            { ServiceError.ErrorCorruptData, "The item is corrupt and cannot be processed" },
            { ServiceError.ErrorMessageSizeExceeded, "The message size exceeds the maximum allowed size" },
            { ServiceError.ErrorMimeContentConversionFailed, "Failed to convert MIME content" },
            { ServiceError.ErrorRecurrenceHasNoOccurrence, "Recurrence pattern has no occurrences" },
            { ServiceError.ErrorInvalidPropertySet, "The property set is invalid" },
            { ServiceError.ErrorInvalidPropertyValue, "The property value is invalid" },
            { ServiceError.ErrorPropertyValidationFailure, "Property validation failed" },
            { ServiceError.ErrorInvalidExtendedProperty, "The extended property definition is invalid" },
            { ServiceError.ErrorInvalidExtendedPropertyValue, "The extended property value is invalid" },
            { ServiceError.ErrorInvalidFolderId, "The folder ID is invalid" },
            { ServiceError.ErrorInvalidId, "The item ID is invalid" },
            { ServiceError.ErrorInvalidRequest, "The request is invalid" },
            { ServiceError.ErrorMailboxMoveInProgress, "Mailbox move is in progress" },
            { ServiceError.ErrorMailboxStoreUnavailable, "Mailbox store is unavailable" },
            { ServiceError.ErrorNameResolutionNoResults, "No results found for name resolution" },
            { ServiceError.ErrorQuotaExceeded, "Mailbox quota exceeded" },
            { ServiceError.ErrorTimeout, "The operation timed out" }
        };

        // Dictionary mapping Exchange service errors to error code types
        private static readonly Dictionary<ServiceError, ErrorCodeType> ServiceErrorToErrorCodeType = new()
        {
            { ServiceError.ErrorAccessDenied, ErrorCodeType.Forbidden },
            { ServiceError.ErrorItemNotFound, ErrorCodeType.NotFound },
            { ServiceError.ErrorFolderNotFound, ErrorCodeType.NotFound },
            { ServiceError.ErrorServerBusy, ErrorCodeType.ServiceUnavailable },
            { ServiceError.ErrorImpersonateUserDenied, ErrorCodeType.Forbidden },
            { ServiceError.ErrorInvalidOperation, ErrorCodeType.BadRequest },
            { ServiceError.ErrorCorruptData, ErrorCodeType.BadRequest },
            { ServiceError.ErrorMessageSizeExceeded, ErrorCodeType.PayloadTooLarge },
            { ServiceError.ErrorMimeContentConversionFailed, ErrorCodeType.BadRequest },
            { ServiceError.ErrorRecurrenceHasNoOccurrence, ErrorCodeType.BadRequest },
            { ServiceError.ErrorInvalidPropertySet, ErrorCodeType.BadRequest },
            { ServiceError.ErrorInvalidPropertyValue, ErrorCodeType.BadRequest },
            { ServiceError.ErrorPropertyValidationFailure, ErrorCodeType.BadRequest },
            { ServiceError.ErrorInvalidExtendedProperty, ErrorCodeType.BadRequest },
            { ServiceError.ErrorInvalidExtendedPropertyValue, ErrorCodeType.BadRequest },
            { ServiceError.ErrorInvalidFolderId, ErrorCodeType.BadRequest },
            { ServiceError.ErrorInvalidId, ErrorCodeType.BadRequest },
            { ServiceError.ErrorInvalidRequest, ErrorCodeType.BadRequest },
            { ServiceError.ErrorMailboxMoveInProgress, ErrorCodeType.ServiceUnavailable },
            { ServiceError.ErrorMailboxStoreUnavailable, ErrorCodeType.ServiceUnavailable },
            { ServiceError.ErrorNameResolutionNoResults, ErrorCodeType.NotFound },
            { ServiceError.ErrorQuotaExceeded, ErrorCodeType.Forbidden },
            { ServiceError.ErrorTimeout, ErrorCodeType.RequestTimeout }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsErrorMappingService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="exceptionFactory">Exception factory</param>
        public EwsErrorMappingService(
            ILogger<EwsErrorMappingService> logger,
            IPiQExceptionFactory exceptionFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        }

        /// <summary>
        /// Maps an Exchange service error to an error code type
        /// </summary>
        public ErrorCodeType MapServiceErrorToErrorCodeType(ServiceError serviceError)
        {
            return ServiceErrorToErrorCodeType.TryGetValue(serviceError, out var errorCodeType)
                ? errorCodeType
                : ErrorCodeType.InternalServerError;
        }

        /// <summary>
        /// Maps a service exception to an application exception
        /// </summary>
        public Exception MapServiceException(ServiceResponseException exception, string? correlationId = null)
        {
            ArgumentNullException.ThrowIfNull(exception);

            // Extract error information from the service response exception
            var errorCode = "ExchangeServiceError";
            var errorMessage = exception.Message;
            ServiceError serviceError = ServiceError.ErrorInternalServerError;

            // Get the error code if available
            serviceError = exception.ErrorCode;
            errorCode = serviceError.ToString();

            if (exception.Response != null && !string.IsNullOrEmpty(exception.Response.ErrorMessage))
            {
                errorMessage = exception.Response.ErrorMessage;
            }

            // Map the error code
            var mappedErrorCode = MapServiceErrorToErrorCodeType(serviceError);

            // Create an exception using the exception factory
            var mappedException = _exceptionFactory.CreateServiceException(
                errorMessage,
                errorCode,
                "ExchangeService",
                "ExecuteOperation",
                exception);

            // Add correlation ID
            if (!string.IsNullOrEmpty(correlationId) && mappedException is PiQException certException)
            {
                certException.SetCorrelationId(correlationId);
            }

            // Log the exception mapping
            _logger.LogDebug(
                "Mapped Exchange service exception with error code {ServiceError} to error code {ErrorCode}",
                serviceError,
                mappedErrorCode);

            return mappedException;
        }

        /// <summary>
        /// Gets a descriptive error message for an Exchange service error
        /// </summary>
        public string GetErrorMessage(ServiceError serviceError)
        {
            return ServiceErrorMessages.TryGetValue(serviceError, out var message)
                ? message
                : $"Exchange service error: {serviceError}";
        }

        /// <summary>
        /// Creates a service exception with Exchange-specific information
        /// </summary>
        public Exception CreateExchangeServiceException(
            string message,
            string errorCode,
            ServiceError? serviceError = null,
            string? correlationId = null,
            Exception? innerException = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            ArgumentException.ThrowIfNullOrEmpty(errorCode);

            // Create properties dictionary with Exchange-specific information
            var properties = new Dictionary<string, object>();

            if (serviceError.HasValue)
            {
                properties.Add("ExchangeServiceError", serviceError.Value);
            }

            // Create an exception using the exception factory
            var exception = _exceptionFactory.CreateServiceException(
                message,
                errorCode,
                "ExchangeService",
                "ExecuteOperation",
                innerException);

            // Add correlation ID
            if (!string.IsNullOrEmpty(correlationId) && exception is PiQException certException)
            {
                certException.SetCorrelationId(correlationId);
            }

            // Log the exception creation
            _logger.LogDebug(
                "Created Exchange service exception with error code {ErrorCode}, service error {ServiceError}",
                errorCode,
                serviceError);

            return exception;
        }
    }
}
