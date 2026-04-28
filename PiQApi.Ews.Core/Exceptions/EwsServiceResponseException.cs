// PiQApi.Ews.Core/Exceptions/EwsServiceResponseException.cs
using Microsoft.Exchange.WebServices.Data;

namespace PiQApi.Ews.Core.Exceptions
{
    /// <summary>
    /// Exception wrapper for Exchange Web Services response exceptions
    /// </summary>
    public class EwsServiceResponseException : EwsServiceException
    {
        /// <summary>
        /// Gets the original ServiceResponseException
        /// </summary>
        public ServiceResponseException? OriginalException { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceResponseException"/> class with default values
        /// </summary>
        public EwsServiceResponseException()
            : base("Exchange Web Services response error occurred")
        {
            // No original exception in this constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceResponseException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        public EwsServiceResponseException(string message)
            : base(message)
        {
            // No original exception in this constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceResponseException"/> class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public EwsServiceResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
            OriginalException = innerException as ServiceResponseException;
        }

        /// <summary>
        /// Creates a new instance of EwsServiceResponseException
        /// </summary>
        /// <param name="originalException">The original ServiceResponseException</param>
        /// <param name="correlationId">Correlation ID associated with the request</param>
        public EwsServiceResponseException(ServiceResponseException originalException, string correlationId)
            : base(originalException, correlationId)
        {
            ArgumentNullException.ThrowIfNull(originalException);
            OriginalException = originalException;

            // Add specific information to AdditionalData
            if (originalException.Response?.ErrorDetails?.Count > 0)
            {
                foreach (var key in originalException.Response.ErrorDetails.Keys)
                {
                    AddData($"ResponseDetail.{key}", originalException.Response.ErrorDetails[key]);
                }
            }
        }
    }
}
