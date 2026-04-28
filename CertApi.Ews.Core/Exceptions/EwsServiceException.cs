// CertApi.Ews.Core/Exceptions/EwsServiceException.cs
using CertApi.Core.Exceptions.Base;
using CertApi.Ews.Core.Enums;
using Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace CertApi.Ews.Core.Exceptions
{
    /// <summary>
    /// Exception wrapper for Exchange Web Services response exceptions
    /// </summary>
    public class EwsServiceException : CertServiceExceptionBase
    {
        /// <summary>
        /// Gets the original ServiceResponseException
        /// </summary>
        public ServiceResponseException? OriginalServiceException { get; }

        /// <summary>
        /// Gets the original ServiceResponse if available
        /// </summary>
        public ServiceResponse? ServiceResponse { get; }

        /// <summary>
        /// Gets the mapped Ews response code
        /// </summary>
        public EwsResponseCodeType EwsResponseCode { get; }

        /// <summary>
        /// Gets whether this is a throttling-related error
        /// </summary>
        public bool IsThrottling { get; }

        /// <summary>
        /// Gets the error code as a string
        /// </summary>
        public string ErrorCodeString { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceException"/> class with default values
        /// </summary>
        public EwsServiceException()
            : base("Exchange Web Services error occurred", "EwsServiceError", "Exchange Web Services", "ProcessRequest")
        {
            ErrorCodeString = "Unknown";
            EwsResponseCode = EwsResponseCodeType.ErrorUnknown;
            IsThrottling = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        public EwsServiceException(string message)
            : base(message, "EwsServiceError", "Exchange Web Services", "ProcessRequest")
        {
            ErrorCodeString = "Unknown";
            EwsResponseCode = EwsResponseCodeType.ErrorUnknown;
            IsThrottling = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public EwsServiceException(string message, Exception innerException)
            : base(message, "EwsServiceError", "Exchange Web Services", "ProcessRequest", innerException)
        {
            ErrorCodeString = "Unknown";
            EwsResponseCode = EwsResponseCodeType.ErrorUnknown;
            IsThrottling = false;
        }

        /// <summary>
        /// Creates a new instance from a ServiceResponseException
        /// </summary>
        /// <param name="exception">The original ServiceResponseException</param>
        /// <param name="correlationId">Correlation ID associated with the request</param>
        public EwsServiceException(ServiceResponseException exception, string correlationId)
            : base(exception?.Message ?? "Exchange service error",
                  "EwsServiceError",
                  "Exchange Web Services",
                  "ProcessResponse",
                  exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            OriginalServiceException = exception;
            ServiceResponse = exception.Response;

            var serviceError = exception.ErrorCode;
            ErrorCodeString = serviceError.ToString();
            EwsResponseCode = MapServiceErrorToEwsResponseCode(serviceError);
            IsThrottling = IsThrottlingError(serviceError);

            // Set correlation ID from parameter
            SetCorrelationId(correlationId);

            // Add error details to AdditionalData
            AddData("EwsResponseCode", EwsResponseCode.ToString());
            AddData("EwsErrorCode", ErrorCodeString);
            if (IsThrottling)
            {
                AddData("IsThrottling", "true");
            }

            // Add error details from the exception
            if (exception.Response?.ErrorDetails?.Count > 0)
            {
                foreach (var key in exception.Response.ErrorDetails.Keys)
                {
                    AddData($"ErrorDetail.{key}", exception.Response.ErrorDetails[key]);
                }
            }
        }

        /// <summary>
        /// Creates a new instance from a ServiceResponse
        /// </summary>
        /// <param name="response">The original ServiceResponse</param>
        /// <param name="correlationId">Correlation ID associated with the request</param>
        public EwsServiceException(ServiceResponse response, string correlationId)
            : base(response?.ErrorMessage ?? "Exchange service error",
                  "EwsServiceError",
                  "Exchange Web Services",
                  "ProcessResponse")
        {
            ArgumentNullException.ThrowIfNull(response);

            OriginalServiceException = null;
            ServiceResponse = response;

            var serviceError = response.ErrorCode;
            ErrorCodeString = serviceError.ToString();
            EwsResponseCode = MapServiceErrorToEwsResponseCode(serviceError);
            IsThrottling = IsThrottlingError(serviceError);

            // Set correlation ID from parameter
            SetCorrelationId(correlationId);

            // Add error details to AdditionalData
            AddData("EwsResponseCode", EwsResponseCode.ToString());
            AddData("EwsErrorCode", ErrorCodeString);
            if (IsThrottling)
            {
                AddData("IsThrottling", "true");
            }

            // Add error details from the response
            if (response.ErrorDetails?.Count > 0)
            {
                foreach (var key in response.ErrorDetails.Keys)
                {
                    AddData($"ErrorDetail.{key}", response.ErrorDetails[key]);
                }
            }
        }

        /// <summary>
        /// Creates a new instance from an exception and web exception
        /// </summary>
        /// <param name="exception">The original exception</param>
        /// <param name="webException">The web exception, if any</param>
        /// <param name="correlationId">Correlation ID associated with the request</param>
        public EwsServiceException(Exception exception, WebException? webException, string correlationId)
            : base(exception?.Message ?? "Exchange service error",
                  "EwsServiceError",
                  "Exchange Web Services",
                  webException != null ? webException.Status.ToString() : "ProcessRequest",
                  exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            OriginalServiceException = exception as ServiceResponseException;
            ServiceResponse = OriginalServiceException?.Response;

            // Set correlation ID from parameter
            SetCorrelationId(correlationId);

            // Determine response code from available information
            if (OriginalServiceException != null)
            {
                var serviceError = OriginalServiceException.ErrorCode;
                ErrorCodeString = serviceError.ToString();
                EwsResponseCode = MapServiceErrorToEwsResponseCode(serviceError);
                IsThrottling = IsThrottlingError(serviceError);
            }
            else if (webException != null)
            {
                // Handle web exception
                ErrorCodeString = webException.Status.ToString();
                IsThrottling = webException.Status == WebExceptionStatus.Timeout ||
                               webException.Status == WebExceptionStatus.RequestCanceled;

                EwsResponseCode = webException.Status switch
                {
                    WebExceptionStatus.Timeout => EwsResponseCodeType.ErrorTimeout,
                    WebExceptionStatus.ConnectFailure => EwsResponseCodeType.ErrorConnectionFailed,
                    WebExceptionStatus.NameResolutionFailure => EwsResponseCodeType.ErrorConnectionFailed,
                    WebExceptionStatus.ProxyNameResolutionFailure => EwsResponseCodeType.ErrorConnectionFailed,
                    WebExceptionStatus.SendFailure => EwsResponseCodeType.ErrorConnectionFailed,
                    WebExceptionStatus.ReceiveFailure => EwsResponseCodeType.ErrorConnectionFailed,
                    WebExceptionStatus.ConnectionClosed => EwsResponseCodeType.ErrorConnectionFailed,
                    WebExceptionStatus.TrustFailure => EwsResponseCodeType.ErrorAccessDenied,
                    WebExceptionStatus.SecureChannelFailure => EwsResponseCodeType.ErrorAccessDenied,
                    WebExceptionStatus.ServerProtocolViolation => EwsResponseCodeType.ErrorSchemaValidation,
                    WebExceptionStatus.RequestCanceled => EwsResponseCodeType.ErrorServerBusy,
                    _ => EwsResponseCodeType.ErrorUnknown
                };
            }
            else
            {
                // Generic error
                ErrorCodeString = "UnknownError";
                EwsResponseCode = EwsResponseCodeType.ErrorUnknown;
                IsThrottling = false;
            }

            // Add error details to AdditionalData
            AddData("EwsResponseCode", EwsResponseCode.ToString());
            AddData("EwsErrorCode", ErrorCodeString);
            if (IsThrottling)
            {
                AddData("IsThrottling", "true");
            }

            // Add error details from service response if available
            if (ServiceResponse?.ErrorDetails?.Count > 0)
            {
                foreach (var key in ServiceResponse.ErrorDetails.Keys)
                {
                    AddData($"ErrorDetail.{key}", ServiceResponse.ErrorDetails[key]);
                }
            }
        }

        /// <summary>
        /// Maps ServiceError to our EwsResponseCodeType
        /// </summary>
        private static EwsResponseCodeType MapServiceErrorToEwsResponseCode(ServiceError serviceError)
        {
            return serviceError switch
            {
                ServiceError.NoError => EwsResponseCodeType.NoError,
                ServiceError.ErrorServerBusy => EwsResponseCodeType.ErrorServerBusy,
                ServiceError.ErrorTimeoutExpired => EwsResponseCodeType.ErrorTimeout,
                ServiceError.ErrorItemNotFound => EwsResponseCodeType.ErrorItemNotFound,
                ServiceError.ErrorAccessDenied => EwsResponseCodeType.ErrorAccessDenied,
                ServiceError.ErrorInvalidAuthorizationContext => EwsResponseCodeType.ErrorInvalidAuthorizationToken,
                ServiceError.ErrorConnectionFailed => EwsResponseCodeType.ErrorConnectionFailed,
                ServiceError.ErrorInvalidOperation => EwsResponseCodeType.ErrorInvalidOperation,
                ServiceError.ErrorSchemaValidation => EwsResponseCodeType.ErrorSchemaValidation,
                ServiceError.ErrorMailboxMoveInProgress => EwsResponseCodeType.ErrorMailboxMoveInProgress,
                ServiceError.ErrorMessageSizeExceeded => EwsResponseCodeType.ErrorSizeLimitExceeded,
                ServiceError.ErrorQuotaExceeded => EwsResponseCodeType.ErrorSizeLimitExceeded,
                ServiceError.ErrorInternalServerError => EwsResponseCodeType.ErrorInternalServerError,
                ServiceError.ErrorMailboxStoreUnavailable => EwsResponseCodeType.ErrorMailboxUnavailable,
                _ => EwsResponseCodeType.ErrorUnknown
            };
        }

        /// <summary>
        /// Determines if an error is throttling-related
        /// </summary>
        private static bool IsThrottlingError(ServiceError serviceError)
        {
            return serviceError switch
            {
                ServiceError.ErrorServerBusy => true,
                ServiceError.ErrorTimeoutExpired => true,
                ServiceError.ErrorExceededConnectionCount => true,
                ServiceError.ErrorExceededSubscriptionCount => true,
                ServiceError.ErrorExceededFindCountLimit => true,
                _ => false
            };
        }
    }
}
