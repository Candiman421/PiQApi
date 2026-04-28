// PiQApi.Ews.Core/Core/EwsCorrelationContext.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Core;
using PiQApi.Core.Utilities.Disposables;
using PiQApi.Ews.Core.Interfaces.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PiQApi.Ews.Core.Core
{
    /// <summary>
    /// Implementation of Exchange Web Services correlation context using composition
    /// </summary>
    public class EwsCorrelationContext : IEwsCorrelationContext
    {
        // Using AsyncLocal ensures context flows across async boundaries
        private static readonly AsyncLocal<EwsCorrelationContext?> _current = new AsyncLocal<EwsCorrelationContext?>();

        private readonly PiQCorrelationContext _coreContext;
        private readonly ILogger<EwsCorrelationContext> _logger;
        private readonly PiQEmptyDisposable _emptyDisposable;
        private string? _tenantId;
        private string? _requestId;
        private string? _userPrincipalName;

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, Exception?> LogContextSet =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1, "SetCurrent"),
                "Set current EWS correlation context with ID: {CorrelationId}");

        private static readonly Action<ILogger, string, string, Exception?> LogTenantIdSet =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(2, "TenantIdSet"),
                "Set tenant ID to {TenantId} for correlation {CorrelationId}");

        private static readonly Action<ILogger, string, string, Exception?> LogRequestIdSet =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(3, "RequestIdSet"),
                "Set request ID to {RequestId} for correlation {CorrelationId}");

        private static readonly Action<ILogger, string, string, Exception?> LogUserPrincipalNameSet =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(4, "UserPrincipalNameSet"),
                "Set user principal name to {UserPrincipalName} for correlation {CorrelationId}");

        private static readonly Action<ILogger, string, string, string, Exception?> LogCreateScope =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Debug,
                new EventId(5, "CreateScope"),
                "Creating EWS correlation scope for operation {OperationName}, correlation ID: {CorrelationId}, tenant ID: {TenantId}");

        private static readonly Action<ILogger, string, Exception, Exception?> LogScopeCreationError =
            LoggerMessage.Define<string, Exception>(
                LogLevel.Warning,
                new EventId(6, "ScopeCreationError"),
                "Failed to create correlation scope, using empty disposable. Operation: {OperationName}");

        /// <summary>
        /// Gets the current correlation context from AsyncLocal storage
        /// </summary>
        public static EwsCorrelationContext? Current => _current.Value;

        /// <summary>
        /// Sets the current context in AsyncLocal storage
        /// </summary>
        /// <param name="context">The context to set as current, or null to clear</param>
        public static void SetCurrent(EwsCorrelationContext? context)
        {
            _current.Value = context;

            // If context is being set, log it
            if (context != null)
            {
                LogContextSet(context._logger, context.CorrelationId, null);
            }
        }

        /// <summary>
        /// Gets the correlation ID
        /// </summary>
        public string CorrelationId => _coreContext.CorrelationId;

        /// <summary>
        /// Gets the creation timestamp
        /// </summary>
        public DateTime CreatedUtc => _coreContext.CreatedUtc;

        /// <summary>
        /// Gets the parent correlation ID
        /// </summary>
        public string? ParentCorrelationId => _coreContext.ParentCorrelationId;

        /// <summary>
        /// Gets the collection of correlation properties
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties => _coreContext.Properties;

        /// <summary>
        /// Gets the tenant ID associated with the correlation
        /// </summary>
        public string? TenantId => _tenantId;

        /// <summary>
        /// Gets the request ID associated with the correlation
        /// </summary>
        public string? RequestId => _requestId;

        /// <summary>
        /// Gets the user principal name associated with the correlation
        /// </summary>
        public string? UserPrincipalName => _userPrincipalName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsCorrelationContext"/> class
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="correlationIdFactory">Correlation ID factory</param>
        /// <param name="logger">Logger</param>
        /// <param name="loggerFactory">Logger factory</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="userPrincipalName">Optional user principal name</param>
        /// <param name="options">Optional configuration options</param>
        public EwsCorrelationContext(
            string correlationId,
            IPiQCorrelationIdFactory correlationIdFactory,
            ILogger<EwsCorrelationContext> logger,
            ILoggerFactory loggerFactory,
            string? tenantId = null,
            string? requestId = null,
            string? userPrincipalName = null,
            IOptions<EwsCorrelationContextOptions>? options = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(correlationId);
            ArgumentNullException.ThrowIfNull(correlationIdFactory);
            ArgumentNullException.ThrowIfNull(loggerFactory);

            // Create options if not provided
            var contextOptions = options?.Value ?? new EwsCorrelationContextOptions();

            // Create a core correlation context with options
            _coreContext = new PiQCorrelationContext(
                correlationIdFactory.CreateFromExisting(correlationId),
                correlationIdFactory,
                loggerFactory.CreateLogger<PiQCorrelationContext>(),
                Options.Create(contextOptions.BaseOptions));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantId = tenantId;
            _requestId = requestId;
            _userPrincipalName = userPrincipalName;
            _emptyDisposable = (PiQEmptyDisposable)PiQEmptyDisposable.Instance;

            // Set the current context in AsyncLocal storage
            SetCurrent(this);

            // Add EWS-specific properties to the context
            if (!string.IsNullOrEmpty(_tenantId))
            {
                AddProperty("TenantId", _tenantId);
            }

            if (!string.IsNullOrEmpty(_requestId))
            {
                AddProperty("RequestId", _requestId);
            }

            if (!string.IsNullOrEmpty(_userPrincipalName))
            {
                AddProperty("UserPrincipalName", _userPrincipalName);
            }

            // Add service type to make it clear this is an EWS context
            AddProperty("ServiceType", "Exchange");

            // Add any additional properties from options
            if (contextOptions.AdditionalProperties.Count > 0)
            {
                AddProperties(contextOptions.AdditionalProperties);
            }
        }

        #region IPiQCorrelationContext Implementation

        /// <summary>
        /// Adds a property to the correlation context
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void AddProperty(string key, object value)
        {
            _coreContext.AddProperty(key, value);
        }

        /// <summary>
        /// Adds multiple properties to the correlation context
        /// </summary>
        /// <param name="properties">Properties to add</param>
        public void AddProperties(IDictionary<string, object> properties)
        {
            _coreContext.AddProperties(properties);
        }

        /// <summary>
        /// Gets a property value
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="key">Property key</param>
        /// <returns>Property value</returns>
        public T GetProperty<T>(string key)
        {
            return _coreContext.GetProperty<T>(key);
        }

        /// <summary>
        /// Tries to get a property value
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="key">Property key</param>
        /// <param name="value">Output property value</param>
        /// <returns>True if property exists and is of type T, otherwise false</returns>
        public bool TryGetProperty<T>(string key, out T? value)
        {
            return _coreContext.TryGetProperty(key, out value);
        }

        /// <summary>
        /// Sets the parent correlation identifier
        /// </summary>
        /// <param name="parentId">Parent correlation ID</param>
        public void SetParentCorrelation(string parentId)
        {
            _coreContext.SetParentCorrelation(parentId);
        }

        /// <summary>
        /// Checks if a property exists
        /// </summary>
        /// <param name="key">Property key</param>
        /// <returns>True if property exists</returns>
        public bool HasProperty(string key)
        {
            return _coreContext.HasProperty(key);
        }

        /// <summary>
        /// Creates a scope with this correlation context
        /// </summary>
        /// <param name="additionalProperties">Additional properties for the scope</param>
        /// <returns>A disposable scope</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Method must create a scope even if the core scope creation fails")]
        public IDisposable CreateScope(IDictionary<string, object>? additionalProperties = null)
        {
            // Add EWS-specific properties to the scope
            var properties = new Dictionary<string, object>(StringComparer.Ordinal);

            if (additionalProperties != null)
            {
                foreach (var kvp in additionalProperties)
                {
                    properties[kvp.Key] = kvp.Value;
                }
            }

            if (!string.IsNullOrEmpty(_tenantId) && !properties.ContainsKey("TenantId"))
            {
                properties["TenantId"] = _tenantId;
            }

            if (!string.IsNullOrEmpty(_requestId) && !properties.ContainsKey("RequestId"))
            {
                properties["RequestId"] = _requestId;
            }

            if (!string.IsNullOrEmpty(_userPrincipalName) && !properties.ContainsKey("UserPrincipalName"))
            {
                properties["UserPrincipalName"] = _userPrincipalName;
            }

            try
            {
                // Create the core scope
                var coreScope = _coreContext.CreateScope();

                // If successful, create the logging scope and combine both
                var loggerScope = _logger.BeginScope(properties);

                // Return a disposable that will clean up both scopes
                return new EwsCorrelationScopeDisposer(coreScope, loggerScope, _logger, CorrelationId, _tenantId, _requestId);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
                _logger.LogWarning(ex, "Failed to create core correlation scope, using empty disposable");

                // If core scope creation fails, just create a logging scope
                var loggerScope = _logger.BeginScope(properties);

                // Use the empty disposable along with the logger scope
                return new EwsCorrelationScopeDisposer(_emptyDisposable, loggerScope, _logger, CorrelationId, _tenantId, _requestId);
            }
        }

        /// <summary>
        /// Creates a scope for a named operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <returns>A disposable scope</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Method must create a scope even if the core scope creation fails")]
        public IDisposable CreateScope(string operationName)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            // Log the scope creation
            LogCreateScope(_logger, operationName, CorrelationId, _tenantId ?? "<none>", null);

            // Create properties dictionary for the logging scope
            var properties = new Dictionary<string, object>
            {
                ["OperationName"] = operationName,
                ["CorrelationId"] = CorrelationId
            };

            if (!string.IsNullOrEmpty(_tenantId))
            {
                properties["TenantId"] = _tenantId;
            }

            if (!string.IsNullOrEmpty(_requestId))
            {
                properties["RequestId"] = _requestId;
            }

            try
            {
                // Use the core context to create the scope
                var coreScope = _coreContext.CreateScope();

                // Create a logging scope
                var loggerScope = _logger.BeginScope(properties);

                // Return a combined disposable
                return new EwsCorrelationScopeDisposer(coreScope, loggerScope, _logger, CorrelationId, _tenantId, _requestId);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
                LogScopeCreationError(_logger, operationName, ex, null);

                // Create just a logging scope
                var loggerScope = _logger.BeginScope(properties);

                // Use the empty disposable for the core scope
                return new EwsCorrelationScopeDisposer(_emptyDisposable, loggerScope, _logger, CorrelationId, _tenantId, _requestId);
            }
        }

        #endregion

        #region IEwsCorrelationContext Implementation

        /// <summary>
        /// Sets the tenant ID
        /// </summary>
        /// <param name="tenantId">Tenant ID to set</param>
        /// <exception cref="ArgumentException">Thrown when tenantId is null or empty</exception>
        public void SetTenantId(string tenantId)
        {
            ArgumentException.ThrowIfNullOrEmpty(tenantId);

            _tenantId = tenantId;
            AddProperty("TenantId", tenantId);

            LogTenantIdSet(_logger, tenantId, CorrelationId, null);
        }

        /// <summary>
        /// Sets the request ID
        /// </summary>
        /// <param name="requestId">Request ID to set</param>
        /// <exception cref="ArgumentException">Thrown when requestId is null or empty</exception>
        public void SetRequestId(string requestId)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            _requestId = requestId;
            AddProperty("RequestId", requestId);

            LogRequestIdSet(_logger, requestId, CorrelationId, null);
        }

        /// <summary>
        /// Sets the user principal name
        /// </summary>
        /// <param name="userPrincipalName">User principal name to set</param>
        /// <exception cref="ArgumentException">Thrown when userPrincipalName is null or empty</exception>
        public void SetUserPrincipalName(string userPrincipalName)
        {
            ArgumentException.ThrowIfNullOrEmpty(userPrincipalName);

            _userPrincipalName = userPrincipalName;
            AddProperty("UserPrincipalName", userPrincipalName);

            LogUserPrincipalNameSet(_logger, userPrincipalName, CorrelationId, null);
        }

        #endregion
    }
}
