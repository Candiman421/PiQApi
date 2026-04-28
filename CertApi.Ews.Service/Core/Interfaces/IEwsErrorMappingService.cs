// CertApi.Ews.Service/Core/Interfaces/IEwsErrorMappingService.cs
using CertApi.Abstractions.Enums;
using Microsoft.Exchange.WebServices.Data;

namespace CertApi.Ews.Service.Core.Interfaces
{
    /// <summary>
    /// Interface for mapping Exchange errors to application errors
    /// </summary>
    public interface IEwsErrorMappingService
    {
        /// <summary>
        /// Maps an Exchange service error to an error code type
        /// </summary>
        /// <param name="serviceError">Exchange service error</param>
        /// <returns>Mapped error code type</returns>
        ErrorCodeType MapServiceErrorToErrorCodeType(ServiceError serviceError);

        /// <summary>
        /// Maps a service exception to an application exception
        /// </summary>
        /// <param name="exception">Service exception</param>
        /// <param name="correlationId">Correlation ID for the exception</param>
        /// <returns>Application exception</returns>
        Exception MapServiceException(ServiceResponseException exception, string? correlationId = null);

        /// <summary>
        /// Gets a descriptive error message for an Exchange service error
        /// </summary>
        /// <param name="serviceError">Exchange service error</param>
        /// <returns>Descriptive error message</returns>
        string GetErrorMessage(ServiceError serviceError);

        /// <summary>
        /// Creates a service exception with Exchange-specific information
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="serviceError">Exchange service error</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="innerException">Inner exception</param>
        /// <returns>Exchange service exception</returns>
        Exception CreateExchangeServiceException(
            string message,
            string errorCode,
            ServiceError? serviceError = null,
            string? correlationId = null,
            Exception? innerException = null);
    }
}