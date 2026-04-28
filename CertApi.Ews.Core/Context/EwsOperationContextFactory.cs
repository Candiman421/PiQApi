// CertApi.Ews.Core/Context/EwsOperationContextFactory.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Validation;
using CertApi.Core.Context;
using CertApi.Ews.Core.Core;
using CertApi.Ews.Core.Interfaces.Context;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Ews.Core.Context
{
    /// <summary>
    /// Factory for creating Ews operation context instances
    /// </summary>
    public class EwsOperationContextFactory : IEwsOperationContextFactory
    {
        private readonly ICertOperationContextFactory _coreContextFactory;
        private readonly ICertCorrelationIdFactory _correlationIdFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICertResultFactory _resultFactory;
        private readonly ICertValidationService _validationService;

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, string, Exception?> LogCreatedContext =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1, nameof(Create)),
                "Created Ews operation context with name {OperationName} and correlation ID {CorrelationId}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsOperationContextFactory"/> class
        /// </summary>
        /// <param name="coreContextFactory">Core operation context factory</param>
        /// <param name="correlationIdFactory">Correlation ID factory</param>
        /// <param name="loggerFactory">Logger factory</param>
        /// <param name="resultFactory">Result factory for creating operation results</param>
        /// <param name="validationService">Validation service</param>
        public EwsOperationContextFactory(
            ICertOperationContextFactory coreContextFactory,
            ICertCorrelationIdFactory correlationIdFactory,
            ILoggerFactory loggerFactory,
            ICertResultFactory resultFactory,
            ICertValidationService validationService)
        {
            _coreContextFactory = coreContextFactory ?? throw new ArgumentNullException(nameof(coreContextFactory));
            _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Creates a basic Ews operation context
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A new Ews operation context</returns>
        public async Task<IEwsOperationContext> CreateAsync(CancellationToken cancellationToken = default)
        {
            // Create a core context
            var coreContext = await _coreContextFactory.CreateInitializedContextAsync(cancellationToken).ConfigureAwait(false);
            
            // Ensure we have the correct type
            if (coreContext is not CertOperationContext certContext)
            {
                throw new InvalidOperationException("Expected CertOperationContext from factory");
            }

            // Create an EWS correlation context
            var ewsCorrelationContext = new EwsCorrelationContext(
                coreContext.CorrelationContext.CorrelationId,
                _correlationIdFactory,
                _loggerFactory.CreateLogger<EwsCorrelationContext>(),
                _loggerFactory);

            // Create EWS metrics
            var metrics = new EwsOperationMetrics(
                coreContext.Metrics,
                _loggerFactory.CreateLogger<EwsOperationMetrics>());

            // Create and return the EWS operation context
            var ewsContext = new EwsOperationContext(
                certContext,
                ewsCorrelationContext,
                metrics,
                _resultFactory,
                _loggerFactory.CreateLogger<EwsOperationContext>());

            return ewsContext;
        }

        /// <summary>
        /// Creates an Ews context with the specified properties
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="ewsCorrelationContext">Ews correlation context</param>
        /// <returns>Ews operation context</returns>
        public IEwsOperationContext Create(string operationName, IEwsCorrelationContext ewsCorrelationContext)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));
            ArgumentNullException.ThrowIfNull(ewsCorrelationContext, nameof(ewsCorrelationContext));

            // Create core context using the correlation context
            var coreContext = _coreContextFactory.Create(operationName, ewsCorrelationContext);
            
            // Ensure we have the correct type
            if (coreContext is not CertOperationContext certContext)
            {
                throw new InvalidOperationException("Expected CertOperationContext from factory");
            }

            // Create EWS metrics
            var metrics = new EwsOperationMetrics(
                coreContext.Metrics,
                _loggerFactory.CreateLogger<EwsOperationMetrics>());

            // Create EWS operation context
            var context = new EwsOperationContext(
                certContext,
                ewsCorrelationContext,
                metrics,
                _resultFactory,
                _loggerFactory.CreateLogger<EwsOperationContext>());

            // Log context creation
            var factoryLogger = _loggerFactory.CreateLogger<EwsOperationContextFactory>();
            LogCreatedContext(factoryLogger, operationName, ewsCorrelationContext.CorrelationId, null);

            return context;
        }

        /// <summary>
        /// Creates a standalone Ews context with its own correlation context
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operationType">Type of operation</param>
        /// <param name="timeout">Operation timeout</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="userPrincipalName">Optional user principal name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Ews operation context</returns>
        public IEwsOperationContext CreateStandalone(
            string operationName,
            OperationType operationType = OperationType.Generic,
            TimeSpan? timeout = null,
            string? tenantId = null,
            string? requestId = null,
            string? userPrincipalName = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

            // Create a new correlation ID
            var correlationId = _correlationIdFactory.Create();
            
            // Create EWS correlation context
            var correlationContextLogger = _loggerFactory.CreateLogger<EwsCorrelationContext>();
            var correlationContext = new EwsCorrelationContext(
                correlationId.Id,
                _correlationIdFactory,
                correlationContextLogger,
                _loggerFactory,
                tenantId,
                requestId,
                userPrincipalName);

            // Create validation context
            var validationContext = _validationService.CreateContext(correlationId.Id, cancellationToken);

            // Create operation ID and name
            var operationId = Guid.NewGuid().ToString("N");
            
            // Create logger for operation context
            var contextLogger = _loggerFactory.CreateLogger<EwsOperationContext>();
            
            // Create EWS operation context
            var context = new EwsOperationContext(
                operationId,
                operationName,
                correlationContext,
                validationContext,
                contextLogger);

            return context;
        }

        /// <summary>
        /// Creates and initializes an Ews operation context
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operationType">Type of operation</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="userPrincipalName">Optional user principal name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Initialized Ews operation context</returns>
        public async Task<IEwsOperationContext> CreateInitializedContextAsync(
            string operationName,
            OperationType operationType = OperationType.Generic,
            string? tenantId = null,
            string? requestId = null,
            string? userPrincipalName = null,
            CancellationToken cancellationToken = default)
        {
            var context = CreateStandalone(
                operationName,
                operationType,
                null,
                tenantId,
                requestId,
                userPrincipalName,
                cancellationToken);

            // Initialize the context
            await context.InitializeAsync(cancellationToken).ConfigureAwait(false);

            return context;
        }
    }
}