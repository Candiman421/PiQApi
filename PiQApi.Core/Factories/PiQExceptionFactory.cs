// PiQApi.Core/Factories/PiQExceptionFactory.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Exceptions;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Core.Exceptions.Infrastructure;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Factories;

/// <summary>
/// Factory for creating and logging exceptions with consistent metadata
/// </summary>
public class PiQExceptionFactory : IPiQExceptionFactory
{
    private readonly ILogger<PiQExceptionFactory> _logger;
    private readonly IPiQCorrelationContext _correlationContext;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, string, string, string, string, Exception?> LogAuthenticationError =
        LoggerMessage.Define<string, string, string, string, string>(
            LogLevel.Error,
            new EventId(1, nameof(CreateAuthenticationException)),
            "[{PiQCorrelationId}] Authentication error for {ServiceUri} with token {TokenId}: {Message} (Code: {ErrorCode})");

    private static readonly Action<ILogger, string, string, object?, string, Exception?> LogBuilderError =
        LoggerMessage.Define<string, string, object?, string>(
            LogLevel.Error,
            new EventId(2, nameof(CreateBuilderException)),
            "[{PiQCorrelationId}] Builder error for property {PropertyName} with value {Value}: {Message}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogConfigurationError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(3, nameof(CreateConfigurationException)),
            "[{PiQCorrelationId}] Configuration error: {Message} (Code: {ErrorCode})");

    private static readonly Action<ILogger, string, string, string, string, Exception?> LogConnectionError =
        LoggerMessage.Define<string, string, string, string>(
            LogLevel.Error,
            new EventId(4, nameof(CreateConnectionException)),
            "[{PiQCorrelationId}] Connection error for {ServiceUri} (RequestId: {RequestId}): {Message}");

    private static readonly Action<ILogger, string, string, string, string, string, Exception?> LogServiceError =
        LoggerMessage.Define<string, string, string, string, string>(
            LogLevel.Error,
            new EventId(5, nameof(CreateServiceException)),
            "[{PiQCorrelationId}] Service error in {ServiceName}.{Operation}: {Message} (Code: {ErrorCode})");

    private static readonly Action<ILogger, string, string, string, Exception?> LogValidationError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(6, nameof(CreateValidationException)),
            "[{PiQCorrelationId}] Validation error for {EntityType}: {Message}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogResourceNotFoundError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(7, nameof(CreateResourceNotFoundException)),
            "[{PiQCorrelationId}] Resource not found of type {ResourceType} with ID {PiQResourceId}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogResourceLockedError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(8, nameof(CreateResourceLockedException)),
            "[{PiQCorrelationId}] Resource {PiQResourceId} is locked by {LockOwner}");

    private static readonly Action<ILogger, string, string, long, long, Exception?> LogResourceQuotaError =
        LoggerMessage.Define<string, string, long, long>(
            LogLevel.Error,
            new EventId(9, nameof(CreateResourceQuotaExceededException)),
            "[{PiQCorrelationId}] Resource quota exceeded for {ResourceType}. Current: {CurrentSize}, Max: {MaxSize}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogMailDeliveryError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(10, nameof(CreateMailDeliveryException)),
            "[{PiQCorrelationId}] Mail delivery error for recipient {RecipientEmail}: {Message}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogPropertyValidationError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(11, nameof(CreatePropertyValidationException)),
            "[{PiQCorrelationId}] Property validation error for {PropertyName}: {Message}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogObjectDisposedError =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(12, nameof(CreateObjectDisposedException)),
            "[{PiQCorrelationId}] Object disposed error for {ObjectName}: {Message}");

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQExceptionFactory"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="correlationContext">The correlation context</param>
    public PiQExceptionFactory(
        ILogger<PiQExceptionFactory> logger,
        IPiQCorrelationContext correlationContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
    }

    /// <summary>
    /// Creates an authentication exception with Uri parameter
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="serviceUri">Service URI</param>
    /// <param name="tokenId">Token ID</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new authentication exception</returns>
    public Exception CreateAuthenticationException(string message, string? errorCode = null, Uri? serviceUri = null, string? tokenId = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        var serviceUriString = serviceUri?.ToString() ?? "null";

        LogAuthenticationError(_logger, correlationId, serviceUriString, tokenId ?? string.Empty, message, errorCode ?? string.Empty, inner);

        var exception = new PiQAuthenticationException(
            message,
            errorCode ?? "AuthenticationFailure",
            serviceUri,  // Pass the Uri directly
            tokenId ?? string.Empty,
            inner);

        // Set correlation ID from current context
        exception.SetCorrelationId(correlationId);

        // Also propagate from inner if available
        PropagateCorrelationId(exception, inner);

        return exception;
    }

    /// <summary>
    /// Creates a builder exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="propertyName">Name of the property</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="attemptedValue">Attempted value</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new builder exception</returns>
    public Exception CreateBuilderException(string message, string propertyName, string? errorCode = null, object? attemptedValue = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogBuilderError(_logger, correlationId, propertyName, attemptedValue, message, inner);

        var exception = new PiQBuilderException(
            message,
            propertyName,
            errorCode ?? "BuilderError",
            attemptedValue,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a configuration exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="errors">List of errors</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new configuration exception</returns>
    public Exception CreateConfigurationException(string message, string? errorCode = null, IEnumerable<string>? errors = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogConfigurationError(_logger, correlationId, message, errorCode ?? "ConfigurationError", inner);

        var exception = new PiQConfigurationException(
            message,
            errorCode,
            errors,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a connection exception with Uri parameter
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="serviceUri">Service URI</param>
    /// <param name="requestId">Request ID</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new connection exception</returns>
    public Exception CreateConnectionException(string message, Uri? serviceUri = null, string? requestId = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        var serviceUriString = serviceUri?.ToString() ?? "null";
        LogConnectionError(_logger, correlationId, serviceUriString, requestId ?? string.Empty, message, inner);

        var exception = new PiQConnectionException(
            message,
            serviceUri,  // Pass the Uri directly
            requestId,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a service exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <param name="serviceName">Service name</param>
    /// <param name="operation">Operation name</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new service exception</returns>
    public Exception CreateServiceException(string message, string? errorCode = null, string? serviceName = null, string? operation = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogServiceError(_logger, correlationId, serviceName ?? string.Empty, operation ?? string.Empty, message, errorCode ?? "ServiceError", inner);

        var exception = new PiQGenericServiceException(
            message,
            errorCode ?? "ServiceError",
            serviceName,
            operation,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a validation exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="entityType">Type of entity</param>
    /// <param name="errors">Validation errors</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new validation exception</returns>
    public Exception CreateValidationException(string message, string? entityType = null, IEnumerable<PiQValidationResult>? errors = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogValidationError(_logger, correlationId, entityType ?? string.Empty, message, inner);

        var exception = new PiQValidationException(
            message,
            entityType ?? string.Empty,
            errors,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a resource not found exception
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="resourceId">Resource ID</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new resource not found exception</returns>
    public Exception CreateResourceNotFoundException(string resourceType, string resourceId, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogResourceNotFoundError(_logger, correlationId, resourceType, resourceId, inner);

        var exception = new PiQResourceNotFoundException(
            resourceType,
            resourceId,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a resource locked exception
    /// </summary>
    /// <param name="resourceId">Resource ID</param>
    /// <param name="lockOwner">Lock owner</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new resource locked exception</returns>
    public Exception CreateResourceLockedException(string resourceId, string? lockOwner = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogResourceLockedError(_logger, correlationId, resourceId, lockOwner ?? string.Empty, inner);

        var exception = new PiQResourceLockedException(
            resourceId,
            lockOwner,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a resource quota exceeded exception
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="currentSize">Current size</param>
    /// <param name="maxSize">Maximum size</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new resource quota exceeded exception</returns>
    public Exception CreateResourceQuotaExceededException(string resourceType, long currentSize, long maxSize, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogResourceQuotaError(_logger, correlationId, resourceType, currentSize, maxSize, inner);

        var exception = new PiQResourceQuotaExceededException(
            resourceType,
            currentSize,
            maxSize,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a mail delivery exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="recipientEmail">Recipient email</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new mail delivery exception</returns>
    public Exception CreateMailDeliveryException(string message, string? recipientEmail = null, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogMailDeliveryError(_logger, correlationId, recipientEmail ?? string.Empty, message, inner);

        var exception = new PiQMailDeliveryException(
            message,
            recipientEmail,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates a property validation exception
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <param name="message">Error message</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new property validation exception</returns>
    public Exception CreatePropertyValidationException(string propertyName, string message, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogPropertyValidationError(_logger, correlationId, propertyName, message, inner);

        var exception = new PiQPropertyValidationException(
            message,
            propertyName,
            inner);

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Creates an object disposed exception
    /// </summary>
    /// <param name="objectName">Object name</param>
    /// <param name="message">Error message</param>
    /// <param name="inner">Inner exception</param>
    /// <returns>A new object disposed exception</returns>
    public Exception CreateObjectDisposedException(string objectName, string message, Exception? inner = null)
    {
        var correlationId = _correlationContext.CorrelationId;
        LogObjectDisposedError(_logger, correlationId, objectName, message, inner);

        var exception = new PiQResourceDisposedException(
            objectName,
            message,
            inner ?? new ObjectDisposedException(objectName, message));

        exception.SetCorrelationId(correlationId);
        PropagateCorrelationId(exception, inner);
        return exception;
    }

    /// <summary>
    /// Propagates correlation ID from an inner exception to the outer exception
    /// </summary>
    /// <param name="exception">The exception to set correlation ID on</param>
    /// <param name="inner">The inner exception to get correlation ID from</param>
    private static void PropagateCorrelationId(PiQException exception, Exception? inner)
    {
        // Only set from inner if not already set and inner has it
        if (string.IsNullOrEmpty(exception.CorrelationId) &&
            inner is IPiQExceptionInfo certEx &&
            !string.IsNullOrEmpty(certEx.CorrelationId))
        {
            exception.SetCorrelationId(certEx.CorrelationId);
        }
    }
}