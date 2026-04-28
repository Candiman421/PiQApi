// PiQApi.Abstractions/Service/Interfaces/IServiceResponse.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Service.Interfaces
{
    /// <summary>
    /// Defines the base interface for all service operation responses
    /// </summary>
    public interface IServiceResponse
    {
        /// <summary>
        /// Gets the status of the service operation
        /// </summary>
        ServiceResultStatusType Status { get; }

        /// <summary>
        /// Gets the error code if operation was not successful
        /// </summary>
        ErrorCodeType ErrorCode { get; }

        /// <summary>
        /// Gets the error message if operation was not successful
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets the timestamp when the response was created
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the correlation ID for tracking the operation
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Gets whether the operation was successful
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Gets the exception that caused the error, if any
        /// </summary>
        Exception? Exception { get; }
    }
}