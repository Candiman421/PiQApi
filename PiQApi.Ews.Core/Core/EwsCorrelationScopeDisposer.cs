// PiQApi.Ews.Core/Core/EwsCorrelationScopeDisposer.cs
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PiQApi.Ews.Core.Core
{
    /// <summary>
    /// Disposable class that cleans up EWS correlation scopes
    /// </summary>
    public sealed class EwsCorrelationScopeDisposer : IDisposable
    {
        private readonly IDisposable _coreScope;
        private readonly IDisposable? _loggingScope;
        private readonly ILogger _logger;
        private readonly string _correlationId;
        private readonly string? _tenantId;
        private bool _disposed;

        // LoggerMessage delegate for better performance
        private static readonly Action<ILogger, string, string?, Exception?> LogScopeDisposed =
            LoggerMessage.Define<string, string?>(
                LogLevel.Debug,
                new EventId(1, "Dispose"),
                "EWS correlation scope disposed. CorrelationId: {CorrelationId}, TenantId: {TenantId}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsCorrelationScopeDisposer"/> class
        /// </summary>
        /// <param name="coreScope">The core scope to dispose</param>
        /// <param name="logger">Logger</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="requestId">Request ID</param>
        public EwsCorrelationScopeDisposer(
            IDisposable coreScope, 
            ILogger logger, 
            string correlationId, 
            string? tenantId, 
            string? requestId)
        {
            _coreScope = coreScope ?? throw new ArgumentNullException(nameof(coreScope));
            _loggingScope = null; // Not used in this constructor overload
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            _tenantId = tenantId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsCorrelationScopeDisposer"/> class
        /// with separate core and logging scopes
        /// </summary>
        /// <param name="coreScope">The core scope to dispose</param>
        /// <param name="loggingScope">The logging scope to dispose</param>
        /// <param name="logger">Logger</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="requestId">Request ID</param>
        public EwsCorrelationScopeDisposer(
            IDisposable coreScope,
            IDisposable? loggingScope,
            ILogger logger,
            string correlationId,
            string? tenantId,
            string? requestId)
        {
            _coreScope = coreScope ?? throw new ArgumentNullException(nameof(coreScope));
            _loggingScope = loggingScope; // Can be null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            _tenantId = tenantId;
        }

        /// <summary>
        /// Disposes the correlation scope
        /// </summary>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", 
            Justification = "Disposal methods should never throw exceptions")]
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                // Dispose the core scope
                _coreScope.Dispose();
                
                // Dispose the logging scope if it was provided
                _loggingScope?.Dispose();

                // Log successful disposal using LoggerMessage delegate
                LogScopeDisposed(_logger, _correlationId, _tenantId, null);
            }
            catch (Exception ex)
            {
                // Don't let exceptions escape from disposal
                _logger.LogWarning(ex, "Error disposing EWS correlation scope. CorrelationId: {CorrelationId}", _correlationId);
            }
        }
    }
}