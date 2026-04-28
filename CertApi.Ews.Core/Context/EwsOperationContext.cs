// CertApi.Ews.Core/Context/EwsOperationContext.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Results;
using CertApi.Abstractions.Validation;
using CertApi.Core.Context;
using CertApi.Ews.Core.Interfaces.Context;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Ews.Core.Context
{
    /// <summary>
    /// EWS-specific implementation of operation context
    /// </summary>
    public partial class EwsOperationContext : IEwsOperationContext
    {
        private readonly CertOperationContext _baseContext;
        private readonly ILogger _logger;
        private readonly IEwsOperationMetrics _metrics;
        private readonly IEwsCorrelationContext _correlationContext;
        private bool _isInProgress;
        private bool _hasCompleted;
        private readonly object _stateLock = new object();

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, string, Exception?> LogOperationStart =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1, nameof(LogOperationStartAsync)),
                "Starting EWS operation {OperationId}, correlation {CorrelationId}");

        private static readonly Action<ILogger, string, string, bool, Exception?> LogOperationEnd =
            LoggerMessage.Define<string, string, bool>(
                LogLevel.Information,
                new EventId(2, nameof(LogOperationEndAsync)),
                "Ending EWS operation {OperationId}, correlation {CorrelationId}, success: {Success}");

        /// <summary>
        /// Gets the operation identifier
        /// </summary>
        public ICertOperationIdentifier Identifier => _baseContext.Identifier;

        /// <summary>
        /// Gets the operation lifecycle manager
        /// </summary>
        public ICertOperationLifecycle Lifecycle => _baseContext;

        /// <summary>
        /// Gets the operation state
        /// </summary>
        public ICertOperationState State => _baseContext.State;

        /// <summary>
        /// Gets the correlation context
        /// </summary>
        public ICertCorrelationContext CorrelationContext => _correlationContext;

        /// <summary>
        /// Gets the validation context
        /// </summary>
        public ICertValidationContext ValidationContext => 
            _baseContext.HasProperty("ValidationContext") ? 
            _baseContext.GetPropertyValue<ICertValidationContext>("ValidationContext") : 
            null;

        /// <summary>
        /// Gets the operation resources
        /// </summary>
        public ICertOperationResources Resources => _baseContext;

        /// <summary>
        /// Gets the operation logger
        /// </summary>
        public ICertOperationLogger Logger => _baseContext;

        /// <summary>
        /// Gets the operation validator
        /// </summary>
        public ICertOperationValidator Validator => null; // Simplified - removed validation requirement

        /// <summary>
        /// Gets the operation metrics
        /// </summary>
        ICertOperationMetrics ICertOperationContext.Metrics => _baseContext.Metrics;

        /// <summary>
        /// Gets the EWS-specific metrics
        /// </summary>
        public IEwsOperationMetrics Metrics => _metrics;

        /// <summary>
        /// Gets the operation ID
        /// </summary>
        public string OperationId => Identifier.Id;

        /// <summary>
        /// Gets the correlation ID
        /// </summary>
        public string CorrelationId => CorrelationContext.CorrelationId;

        /// <summary>
        /// Gets the properties collection
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties => _baseContext.Properties;

        /// <summary>
        /// Gets the cancellation token
        /// </summary>
        public CancellationToken CancellationToken => _baseContext.CancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsOperationContext"/> class
        /// </summary>
        /// <param name="baseContext">Base context implementation</param>
        /// <param name="correlationContext">EWS correlation context</param>
        /// <param name="metrics">EWS-specific metrics</param>
        /// <param name="logger">Logger</param>
        public EwsOperationContext(
            CertOperationContext baseContext,
            IEwsCorrelationContext correlationContext,
            IEwsOperationMetrics metrics,
            ILogger logger)
        {
            _baseContext = baseContext ?? throw new ArgumentNullException(nameof(baseContext));
            _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates an operation context with specified parameters
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="correlationContext">Correlation context</param>
        /// <param name="logger">Logger</param>
        public EwsOperationContext(
            string operationId,
            string operationName,
            ICertCorrelationContext correlationContext,
            ILogger logger)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationId, nameof(operationId));
            ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));
            ArgumentNullException.ThrowIfNull(correlationContext, nameof(correlationContext));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            // Simple constructor stub for now
            _logger = logger;
            _correlationContext = correlationContext as IEwsCorrelationContext ?? 
                throw new ArgumentException("Correlation context must implement IEwsCorrelationContext", nameof(correlationContext));
            _metrics = new EwsOperationMetrics();
            
            // Will need to be initialized properly in a separate call
            _baseContext = null;
        }

        /// <summary>
        /// Logs the start of an operation
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task LogOperationStartAsync()
        {
            lock (_stateLock)
            {
                if (_isInProgress)
                {
                    throw new InvalidOperationException($"Operation {OperationId} is already in progress");
                }

                _isInProgress = true;
            }

            // Log the start using the delegate
            LogOperationStart(_logger, OperationId, CorrelationId, null);

            // Start the timer for this operation
            _metrics.StartTimer(OperationId);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Logs the end of an operation
        /// </summary>
        /// <param name="success">Whether the operation was successful</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task LogOperationEndAsync(bool success)
        {
            lock (_stateLock)
            {
                if (_hasCompleted)
                {
                    // Already completed, just return
                    return;
                }

                if (!_isInProgress)
                {
                    throw new InvalidOperationException($"Operation {OperationId} has not been started");
                }

                _hasCompleted = true;
            }

            // Stop the timer and record the result
            var duration = _metrics.StopTimer(OperationId);
            _metrics.RecordOperation(OperationId, duration, success);

            // Log the end using the delegate
            LogOperationEnd(_logger, OperationId, CorrelationId, success, null);

            // Update state
            if (_baseContext != null)
            {
                if (success)
                {
                    _baseContext.UpdateState(OperationStatusType.Done);
                }
                else
                {
                    _baseContext.UpdateState(OperationStatusType.Failed);
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current context as active
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SetCurrentAsync(CancellationToken cancellationToken = default)
        {
            // Use the CorrelationContext to create a scope
            using var scope = _correlationContext.CreateScope();
            
            // Add operation properties to the scope
            var properties = new Dictionary<string, object>
            {
                ["OperationId"] = OperationId,
                ["CorrelationId"] = CorrelationId
            };
            
            // Create a logging scope
            using var loggingScope = _logger.BeginScope(properties);
            
            // Wait a bit to ensure context is established
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates an operation scope
        /// </summary>
        /// <returns>A disposable scope</returns>
        public IDisposable CreateScope()
        {
            return _baseContext?.CreateScope() ?? new DummyScope();
        }

        /// <summary>
        /// Creates an operation scope with the specified name
        /// </summary>
        /// <param name="name">Scope name</param>
        /// <returns>A disposable scope</returns>
        public IDisposable CreateScope(string name)
        {
            return _baseContext?.CreateScope(name) ?? new DummyScope();
        }

        #region ICertOperationProperties Implementation

        /// <summary>
        /// Adds a property to the operation context
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void AddProperty(string key, object value)
        {
            _baseContext?.AddProperty(key, value);
        }

        /// <summary>
        /// Adds multiple properties to the operation context
        /// </summary>
        /// <param name="properties">Properties to add</param>
        public void AddProperties(IDictionary<string, object> properties)
        {
            _baseContext?.AddProperties(properties);
        }

        /// <summary>
        /// Gets a property value
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="key">Property key</param>
        /// <returns>Property value</returns>
        public T GetPropertyValue<T>(string key)
        {
            return _baseContext != null 
                ? _baseContext.GetPropertyValue<T>(key)
                : default;
        }

        /// <summary>
        /// Tries to get a property value
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="key">Property key</param>
        /// <param name="value">Output property value</param>
        /// <returns>True if property exists and can be cast to T; otherwise, false</returns>
        public bool TryGetPropertyValue<T>(string key, out T? value)
        {
            if (_baseContext != null)
            {
                return _baseContext.TryGetPropertyValue(key, out value);
            }
            
            value = default;
            return false;
        }

        /// <summary>
        /// Checks if a property exists
        /// </summary>
        /// <param name="key">Property key</param>
        /// <returns>True if property exists; otherwise, false</returns>
        public bool HasProperty(string key)
        {
            return _baseContext?.HasProperty(key) ?? false;
        }

        #endregion

        #region ICertOperationLifecycle Implementation

        /// <summary>
        /// Initializes the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return _baseContext?.InitializeAsync(cancellationToken) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Validates the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task ValidateAsync(CancellationToken cancellationToken)
        {
            return _baseContext?.ValidateAsync(cancellationToken) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Starts the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_baseContext != null)
            {
                _baseContext.UpdateState(OperationStatusType.Running);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Completes the operation
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task CompleteAsync()
        {
            if (_baseContext != null)
            {
                _baseContext.UpdateState(OperationStatusType.Done);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Marks the operation as failed
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task FailAsync()
        {
            if (_baseContext != null)
            {
                _baseContext.UpdateState(OperationStatusType.Failed);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a child context
        /// </summary>
        /// <param name="childOperationName">Child operation name</param>
        /// <returns>A child operation context</returns>
        public ICertOperationContext CreateChildContext(string childOperationName)
        {
            return _baseContext?.CreateChildContext(childOperationName) ?? this;
        }

        #endregion

        #region ICertOperationLogger Implementation

        /// <summary>
        /// Logs an operation error
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task LogOperationErrorAsync(Exception exception)
        {
            return _baseContext?.LogOperationErrorAsync(exception) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Creates a logging scope
        /// </summary>
        /// <param name="scopeProperties">Scope properties</param>
        /// <returns>A disposable scope</returns>
        public IDisposable CreateLoggingScope(IDictionary<string, object>? scopeProperties = null)
        {
            return _baseContext?.CreateLoggingScope(scopeProperties) ?? new DummyScope();
        }

        /// <summary>
        /// Checks if logging is enabled for the specified level
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <returns>True if logging is enabled; otherwise, false</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _baseContext?.IsEnabled(logLevel) ?? _logger.IsEnabled(logLevel);
        }

        #endregion

        #region ICertOperationValidator Implementation

        /// <summary>
        /// Validates an object
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="entity">Object to validate</param>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<ICertValidationResult> ValidateAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken) where T : class
        {
            // Simplified - no validation
            throw new NotSupportedException("Validation is not supported in simplified mode");
        }

        /// <summary>
        /// Validates an object with required mode
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="entity">Object to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<ICertValidationResult> ValidateRequiredAsync<T>(T entity, CancellationToken cancellationToken) where T : class
        {
            // Simplified - no validation
            throw new NotSupportedException("Validation is not supported in simplified mode");
        }

        /// <summary>
        /// Validates an object and creates a result
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="entity">Object to validate</param>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<ICertResult<T>> ValidateAndCreateResultAsync<T>(T entity, ValidationModeType mode, CancellationToken cancellationToken) where T : class
        {
            // Simplified - no validation
            throw new NotSupportedException("Validation is not supported in simplified mode");
        }

        /// <summary>
        /// Creates a validation context
        /// </summary>
        /// <param name="mode">Validation mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A validation context</returns>
        public ICertValidationContext CreateValidationContext(ValidationModeType mode = ValidationModeType.Standard, CancellationToken? cancellationToken = null)
        {
            // Simplified - no validation
            throw new NotSupportedException("Validation is not supported in simplified mode");
        }

        #endregion

        #region ICertOperationResources Implementation

        /// <summary>
        /// Gets a resource
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resourceId">Resource ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<T> GetResourceAsync<T>(string resourceId, CancellationToken cancellationToken) where T : class, ICertResource
        {
            return _baseContext?.GetResourceAsync<T>(resourceId, cancellationToken) ?? 
                   Task.FromException<T>(new NotSupportedException("Resources not supported in simplified mode"));
        }

        /// <summary>
        /// Tracks a resource
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<T> TrackResourceAsync<T>(T resource, CancellationToken cancellationToken) where T : class, ICertResource
        {
            return _baseContext?.TrackResourceAsync(resource, cancellationToken) ?? Task.FromResult(resource);
        }

        /// <summary>
        /// Tries to acquire a resource
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<bool> TryAcquireResourceAsync(string resourceId, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return _baseContext?.TryAcquireResourceAsync(resourceId, timeout, cancellationToken) ?? 
                   Task.FromResult(false);
        }

        /// <summary>
        /// Releases a resource
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task ReleaseResourceAsync(string resourceId, CancellationToken cancellationToken)
        {
            return _baseContext?.ReleaseResourceAsync(resourceId, cancellationToken) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Creates a resource scope
        /// </summary>
        /// <param name="resourceIds">Resource IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<ICertResourceScope> CreateResourceScopeAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken)
        {
            return _baseContext?.CreateResourceScopeAsync(resourceIds, cancellationToken) ?? 
                   Task.FromException<ICertResourceScope>(new NotSupportedException("Resources not supported in simplified mode"));
        }

#endregion
    }

    
}
