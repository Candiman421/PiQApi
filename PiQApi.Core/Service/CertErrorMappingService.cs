// PiQApi.Core/Service/CertErrorMappingService.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Service;

/// <summary>
/// Base service for mapping service errors to application errors
/// </summary>
public class CertErrorMappingService : ICertErrorMappingService
{
    private readonly ILogger<CertErrorMappingService> _logger;
    private readonly ICertExceptionFactory _exceptionFactory;
    private readonly ICertCorrelationContext _correlationContext;

    // Define LoggerMessage delegate for better performance
    private static readonly Action<ILogger, string, string, string, Exception?> LogServiceError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(100, "ServiceError"),
            "[{CorrelationId}] Service error: Code={ErrorCode}, Message={Message}");

    /// <summary>
    /// Initializes a new instance of the <see cref="CertErrorMappingService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="exceptionFactory">The exception factory</param>
    /// <param name="correlationContext">The correlation context</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public CertErrorMappingService(
        ILogger<CertErrorMappingService> logger,
        ICertExceptionFactory exceptionFactory,
        ICertCorrelationContext correlationContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
    }

    /// <summary>
    /// Maps a service error to an ErrorCodeType
    /// </summary>
    public virtual ErrorCodeType MapServiceErrorToErrorCodeType(ErrorCodeType serviceError)
    {
        // Map service-specific error codes to application error codes
        return serviceError switch
        {
            // Service-specific codes that need mapping to application codes
            ErrorCodeType.AccessDenied => ErrorCodeType.AuthenticationFailure,
            ErrorCodeType.ConnectionFailed => ErrorCodeType.ConnectionError,
            ErrorCodeType.TimeoutExpired => ErrorCodeType.TimeoutError,
            ErrorCodeType.ItemNotFound => ErrorCodeType.NotFound,
            ErrorCodeType.NoError => ErrorCodeType.None,
            ErrorCodeType.ServerBusy => ErrorCodeType.ServiceUnavailable,
            ErrorCodeType.ServiceUnavailable => ErrorCodeType.ServiceUnavailable,
            ErrorCodeType.TooManyRequests => ErrorCodeType.RateLimitExceeded,
            ErrorCodeType.InvalidRequest => ErrorCodeType.ValidationError,

            // For application error codes, just return them as-is
            ErrorCodeType.None => ErrorCodeType.None,
            ErrorCodeType.AuthenticationFailure => ErrorCodeType.AuthenticationFailure,
            ErrorCodeType.AuthenticationError => ErrorCodeType.AuthenticationError,
            ErrorCodeType.ConnectionError => ErrorCodeType.ConnectionError,
            ErrorCodeType.NotFound => ErrorCodeType.NotFound,
            ErrorCodeType.ValidationError => ErrorCodeType.ValidationError,
            ErrorCodeType.Unauthorized => ErrorCodeType.Unauthorized,
            ErrorCodeType.InvalidOperation => ErrorCodeType.InvalidOperation,
            ErrorCodeType.InternalServerError => ErrorCodeType.InternalServerError,
            ErrorCodeType.ConfigurationError => ErrorCodeType.ConfigurationError,
            ErrorCodeType.TimeoutError => ErrorCodeType.TimeoutError,
            ErrorCodeType.RateLimitExceeded => ErrorCodeType.RateLimitExceeded,
            ErrorCodeType.Unknown => ErrorCodeType.Unknown,
            ErrorCodeType.ServiceError => ErrorCodeType.ServiceError,

            // Default case for any unmapped codes
            _ => ErrorCodeType.ServiceError,
        };
    }

    /// <summary>
    /// Creates a service exception from a response exception
    /// </summary>
    public virtual Exception CreateServiceExceptionFromResponse(Exception responseException)
    {
        ArgumentNullException.ThrowIfNull(responseException);

        var correlationId = _correlationContext.CorrelationId;
        var errorCode = MapSystemExceptionToErrorCodeType(responseException);

        // Track error in correlation context
        _correlationContext.AddProperty("ErrorOperation", "ServiceExceptionMapping");
        _correlationContext.AddProperty("ErrorCode", errorCode.ToString());

        LogServiceError(_logger, correlationId, errorCode.ToString(), responseException.Message, responseException);

        // Add correlation info to exception data if not already present
        if (!responseException.Data.Contains("CorrelationId"))
        {
            responseException.Data["CorrelationId"] = correlationId;
        }

        // Create the appropriate exception based on the error code
        return errorCode switch
        {
            ErrorCodeType.AuthenticationFailure =>
                _exceptionFactory.CreateAuthenticationException(
                    responseException.Message,
                    errorCode.ToString(),
                    null, // serviceUri
                    null, // tokenId
                    responseException),

            ErrorCodeType.ConnectionError =>
                _exceptionFactory.CreateConnectionException(
                    responseException.Message,
                    null, // serviceUri
                    null, // requestId
                    responseException),

            ErrorCodeType.None =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    errorCode.ToString(),
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.ValidationError =>
                _exceptionFactory.CreateValidationException(
                    responseException.Message,
                    "Service",
                    null, // errors
                    responseException),

            ErrorCodeType.AuthenticationError =>
                _exceptionFactory.CreateAuthenticationException(
                    responseException.Message,
                    errorCode.ToString(),
                    null, // serviceUri
                    null, // tokenId
                    responseException),

            ErrorCodeType.Unauthorized =>
                _exceptionFactory.CreateAuthenticationException(
                    responseException.Message,
                    errorCode.ToString(),
                    null, // serviceUri
                    null, // tokenId
                    responseException),

            ErrorCodeType.AccessDenied =>
                _exceptionFactory.CreateAuthenticationException(
                    responseException.Message,
                    "AccessDenied",
                    null, // serviceUri
                    null, // tokenId
                    responseException),

            ErrorCodeType.NotFound =>
                _exceptionFactory.CreateResourceNotFoundException(
                    "Resource",
                    errorCode.ToString(),
                    responseException),

            ErrorCodeType.InvalidOperation =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    errorCode.ToString(),
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.InternalServerError =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    errorCode.ToString(),
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.ServiceUnavailable =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    "ServiceUnavailable",
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.ConfigurationError =>
                _exceptionFactory.CreateConfigurationException(
                    responseException.Message,
                    errorCode.ToString(),
                    null, // errors
                    responseException),

            ErrorCodeType.TimeoutError =>
                _exceptionFactory.CreateConnectionException(
                    responseException.Message,
                    null, // serviceUri
                    errorCode.ToString(), // using errorCode as requestId for tracking
                    responseException),

            ErrorCodeType.RateLimitExceeded =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    "TooManyRequests",
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.Unknown =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    "UnknownError",
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.ServiceError =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    "ServiceError",
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.ConnectionFailed =>
                _exceptionFactory.CreateConnectionException(
                    responseException.Message,
                    null, // serviceUri
                    "ConnectionFailed", // requestId
                    responseException),

            ErrorCodeType.TimeoutExpired =>
                _exceptionFactory.CreateConnectionException(
                    responseException.Message,
                    null, // serviceUri
                    "TimeoutExpired", // requestId
                    responseException),

            ErrorCodeType.ItemNotFound =>
                _exceptionFactory.CreateResourceNotFoundException(
                    "Resource",
                    "ItemNotFound",
                    responseException),

            ErrorCodeType.NoError =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    "NoError",
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.ServerBusy =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    "ServerBusy",
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.TooManyRequests =>
                _exceptionFactory.CreateServiceException(
                    responseException.Message,
                    "TooManyRequests",
                    "Service",
                    null, // operation
                    responseException),

            ErrorCodeType.InvalidRequest =>
                _exceptionFactory.CreateValidationException(
                    responseException.Message,
                    "Service",
                    null, // errors
                    responseException),

            _ => _exceptionFactory.CreateServiceException(
                responseException.Message,
                errorCode.ToString(),
                "Service",
                null, // operation
                responseException)
        };
    }

    /// <summary>
    /// Maps a system exception to an ErrorCodeType
    /// </summary>
    public virtual ErrorCodeType MapSystemExceptionToErrorCodeType(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            UnauthorizedAccessException => ErrorCodeType.Unauthorized,
            TimeoutException => ErrorCodeType.TimeoutError,
            ArgumentException => ErrorCodeType.ValidationError,
            InvalidOperationException => ErrorCodeType.InvalidOperation,
            _ => ErrorCodeType.InternalServerError
        };
    }
}