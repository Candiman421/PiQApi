// CertApi.Ews.Core/Validation/EwsValidationContextFactory.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Utilities.Time;
using CertApi.Abstractions.Validation;
using CertApi.Ews.Core.Validation.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace CertApi.Ews.Core.Validation
{
    /// <summary>
    /// Factory for creating EWS-specific validation contexts
    /// </summary>
    public class EwsValidationContextFactory : IEwsValidationContextFactory
    {
        private readonly ICertCorrelationIdFactory _correlationIdFactory;
        private readonly ILogger<EwsValidationContextFactory> _logger;
        private readonly ICertTimeProvider _timeProvider;

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, ExchangeVersion, Exception?> LogCreateDefault =
            LoggerMessage.Define<ExchangeVersion>(
                LogLevel.Debug,
                new EventId(1, "CreateForEwsService"),
                "Creating EWS validation context for Exchange version: {ExchangeVersion}");

        private static readonly Action<ILogger, string, ExchangeVersion, Exception?> LogCreateWithCorrelationId =
            LoggerMessage.Define<string, ExchangeVersion>(
                LogLevel.Debug,
                new EventId(2, "CreateForEwsServiceWithId"),
                "Creating EWS validation context with correlation ID: {CorrelationId}, Exchange version: {ExchangeVersion}");

        private static readonly Action<ILogger, string, ExchangeVersion, Exception?> LogCreateWithScope =
            LoggerMessage.Define<string, ExchangeVersion>(
                LogLevel.Debug,
                new EventId(3, "CreateWithScope"),
                "Creating EWS validation context with scope: {Scope}, Exchange version: {ExchangeVersion}");

        private static readonly Action<ILogger, ExchangeVersion, Exception?> LogCreateForExtendedProperties =
            LoggerMessage.Define<ExchangeVersion>(
                LogLevel.Debug,
                new EventId(4, "CreateForExtendedProperties"),
                "Creating EWS validation context for extended properties, Exchange version: {ExchangeVersion}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsValidationContextFactory"/> class
        /// </summary>
        /// <param name="correlationIdFactory">Factory for creating correlation IDs</param>
        /// <param name="logger">Logger for validation events</param>
        /// <param name="timeProvider">Time provider</param>
        public EwsValidationContextFactory(
            ICertCorrelationIdFactory correlationIdFactory,
            ILogger<EwsValidationContextFactory> logger,
            ICertTimeProvider timeProvider)
        {
            _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        /// <summary>
        /// Creates a validation context for EWS service operations
        /// </summary>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context configured for EWS</returns>
        public IEwsValidationContext CreateForEwsService(
            ExchangeVersion exchangeVersion,
            CancellationToken cancellationToken = default)
        {
            LogCreateDefault(_logger, exchangeVersion, null);
            
            // Create a new correlation ID using the factory
            var correlationId = _correlationIdFactory.Create();
            
            return new EwsValidationContext(
                ValidationModeType.Standard,
                0, // depth
                10, // maxDepth
                false, // aggregateAllErrors
                correlationId.Id,
                ImmutableDictionary<string, object>.Empty,
                _timeProvider,
                exchangeVersion,
                null, // ewsScope
                false, // validatePermissions
                true, // validateExtendedProperties
                true, // validateWellKnownFolders
                cancellationToken);
        }

        /// <summary>
        /// Creates a validation context for EWS service operations with correlation ID
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context configured for EWS with the specified correlation ID</returns>
        public IEwsValidationContext CreateForEwsService(
            string correlationId,
            ExchangeVersion exchangeVersion,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(correlationId);
            
            LogCreateWithCorrelationId(_logger, correlationId, exchangeVersion, null);

            return new EwsValidationContext(
                ValidationModeType.Standard,
                0, // depth
                10, // maxDepth
                false, // aggregateAllErrors
                correlationId,
                ImmutableDictionary<string, object>.Empty,
                _timeProvider,
                exchangeVersion,
                null, // ewsScope
                false, // validatePermissions
                true, // validateExtendedProperties
                true, // validateWellKnownFolders
                cancellationToken);
        }

        /// <summary>
        /// Creates a validation context for EWS operations with specified scope
        /// </summary>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="scope">Validation scope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context configured for EWS with the specified scope</returns>
        public IEwsValidationContext CreateWithScope(
            ExchangeVersion exchangeVersion,
            string scope,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(scope);
            
            LogCreateWithScope(_logger, scope, exchangeVersion, null);

            // Create a new correlation ID using the factory
            var correlationId = _correlationIdFactory.Create();

            return new EwsValidationContext(
                ValidationModeType.Standard,
                0, // depth
                10, // maxDepth
                false, // aggregateAllErrors
                correlationId.Id,
                ImmutableDictionary<string, object>.Empty,
                _timeProvider,
                exchangeVersion,
                scope, // ewsScope
                false, // validatePermissions
                true, // validateExtendedProperties
                true, // validateWellKnownFolders
                cancellationToken);
        }

        /// <summary>
        /// Creates a validation context for extended property validation
        /// </summary>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation context configured for extended property validation</returns>
        public IEwsValidationContext CreateForExtendedProperties(
            ExchangeVersion exchangeVersion,
            CancellationToken cancellationToken = default)
        {
            LogCreateForExtendedProperties(_logger, exchangeVersion, null);

            // Create a new correlation ID using the factory
            var correlationId = _correlationIdFactory.Create();
            
            return new EwsValidationContext(
                ValidationModeType.Standard,
                0, // depth
                10, // maxDepth
                false, // aggregateAllErrors
                correlationId.Id,
                ImmutableDictionary<string, object>.Empty,
                _timeProvider,
                exchangeVersion,
                "ExtendedProperties", // ewsScope
                false, // validatePermissions
                true, // validateExtendedProperties
                false, // validateWellKnownFolders
                cancellationToken);
        }
    }
}