// CertApi.Abstractions/Factories/IExceptionFactory.cs
using CertApi.Abstractions.Validation;

namespace CertApi.Abstractions.Factories
{
    /// <summary>
    /// Factory for creating exceptions with proper logging and context
    /// </summary>
    public interface IExceptionFactory
    {
        /// <summary>
        /// Creates an authentication exception
        /// </summary>
        Exception CreateAuthenticationException(string message, string errorCode, string serviceUrl, string tokenId, Exception? inner = null);

        /// <summary>
        /// Creates an authentication exception with Uri parameter
        /// </summary>
        Exception CreateAuthenticationException(string message, string errorCode, Uri serviceUri, string tokenId, Exception? inner = null);

        /// <summary>
        /// Creates a builder exception
        /// </summary>
        Exception CreateBuilderException(string message, string propertyName, string? errorCode = null, object? attemptedValue = null, Exception? inner = null);

        /// <summary>
        /// Creates a configuration exception
        /// </summary>
        Exception CreateConfigurationException(string message, string? errorCode = null, IEnumerable<string>? errors = null, Exception? inner = null);

        /// <summary>
        /// Creates a connection exception
        /// </summary>
        Exception CreateConnectionException(string message, string? serviceUrl = null, string? requestId = null, Exception? inner = null);

        /// <summary>
        /// Creates a connection exception with Uri parameter
        /// </summary>
        Exception CreateConnectionException(string message, Uri? serviceUri = null, string? requestId = null, Exception? inner = null);

        /// <summary>
        /// Creates a service exception
        /// </summary>
        Exception CreateServiceException(string message, string? errorCode = null, string? serviceName = null, string? operation = null, Exception? inner = null);

        /// <summary>
        /// Creates a validation exception
        /// </summary>
        Exception CreateValidationException(string message, string? entityType = null, IEnumerable<CertValidationResult>? errors = null, Exception? inner = null);

        /// <summary>
        /// Creates a resource not found exception
        /// </summary>
        Exception CreateResourceNotFoundException(string resourceType, string resourceId, Exception? inner = null);

        /// <summary>
        /// Creates a resource locked exception
        /// </summary>
        Exception CreateResourceLockedException(string resourceId, string? lockOwner = null, Exception? inner = null);

        /// <summary>
        /// Creates a resource quota exceeded exception
        /// </summary>
        Exception CreateResourceQuotaExceededException(string resourceType, long currentSize, long maxSize, Exception? inner = null);

        /// <summary>
        /// Creates a mail delivery exception
        /// </summary>
        Exception CreateMailDeliveryException(string message, string? recipientEmail = null, Exception? inner = null);
    }
}