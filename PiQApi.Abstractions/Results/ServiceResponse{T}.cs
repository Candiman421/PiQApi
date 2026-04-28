// PiQApi.Abstractions/Results/ServiceResponse{T}.cs
// PiQApi.Abstractions/Results/ServiceResponse{T}.cs
// PiQApi.Abstractions/Results/ServiceResponse{T}.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Service.Interfaces;

namespace PiQApi.Abstractions.Results
{
    /// <summary>
    /// Implementation of service response for generic type T
    /// </summary>
    internal sealed class ServiceResponse<T> : IServiceResponse<T> where T : class
    {
        public ServiceResultStatusType Status { get; }
        public ErrorCodeType ErrorCode { get; }
        public string ErrorMessage { get; }
        public DateTimeOffset Timestamp { get; }
        public string CorrelationId { get; }
        public bool IsSuccess => Status == ServiceResultStatusType.Success;
        public T? Result { get; }
        public Exception? Exception { get; }

        public ServiceResponse(
            T? result,
            ServiceResultStatusType status,
            ErrorCodeType errorCode,
            string errorMessage,
            string correlationId,
            Exception? exception = null)
        {
            Result = result;
            Status = status;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage ?? string.Empty;
            CorrelationId = correlationId ?? string.Empty;
            Timestamp = DateTimeOffset.UtcNow;
            Exception = exception;
        }
    }
}