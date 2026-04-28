// PiQApi.Ews.Service/Core/EwsServiceFactory.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Authentication;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Ews.Core.Enums;
using PiQApi.Ews.Core.Interfaces;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Service.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PiQApi.Ews.Service.Core
{
    /// <summary>
    /// Factory for creating EWS service instances
    /// </summary>
    public class EwsServiceFactory : IEwsServiceFactory
    {
        private readonly ILogger<EwsServiceFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICertExceptionFactory _exceptionFactory;
        private readonly IEwsErrorMappingService _errorMappingService;
        private readonly IEwsPolicyExecutor _policyExecutor;
        private readonly IEwsPolicyTypeMapper _policyTypeMapper;
        private readonly EwsServiceOptions _options;

        // LoggerMessage delegates for high-performance logging
        private static readonly Action<ILogger, string, Exception?> _logCreatingService =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1, nameof(CreateServiceAsync)),
                "Creating new EWS service. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, string, string, Exception?> _logCreatingServiceWithImpersonation =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(2, nameof(CreateServiceWithImpersonationAsync)),
                "Creating new EWS service with impersonation for user {User}. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception> _logCreationError =
            LoggerMessage.Define<string, Exception>(
                LogLevel.Error,
                new EventId(3, nameof(CreateServiceAsync)),
                "Error creating EWS service: {ErrorMessage}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceFactory"/> class
        /// </summary>
        public EwsServiceFactory(
            ILogger<EwsServiceFactory> logger,
            ILoggerFactory loggerFactory,
            ICertExceptionFactory exceptionFactory,
            IEwsErrorMappingService errorMappingService,
            IEwsPolicyExecutor policyExecutor,
            IEwsPolicyTypeMapper EwsPolicyTypeMapper,
            IOptions<EwsServiceOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            _errorMappingService = errorMappingService ?? throw new ArgumentNullException(nameof(errorMappingService));
            _policyExecutor = policyExecutor ?? throw new ArgumentNullException(nameof(policyExecutor));
            _policyTypeMapper = EwsPolicyTypeMapper ?? throw new ArgumentNullException(nameof(policyTypeMapper));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Creates a new EWS service
        /// </summary>
        public async Task<IEwsServiceBase> CreateServiceAsync(IEwsOperationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            _logCreatingService(_logger, context.CorrelationId, null);

            try
            {
                // Create the underlying service wrapper - this is the implementation detail
                var serviceWrapper = await CreateServiceWrapperAsync(cancellationToken).ConfigureAwait(false);

                // Set correlation ID
                serviceWrapper.SetCorrelationId(context.CorrelationId);

                // Configure the service
                await ConfigureServiceUrlAsync(serviceWrapper, cancellationToken).ConfigureAwait(false);
                await ConfigureAuthenticationAsync(serviceWrapper, context, cancellationToken).ConfigureAwait(false);

                // Create the EWS service implementation that uses the wrapper
                var serviceLogger = _loggerFactory.CreateLogger<EwsServiceBase>();

                return new EwsServiceImplementation(
                    serviceWrapper,
                    _errorMappingService,
                    _policyExecutor,
                    _exceptionFactory,
                    serviceLogger,
                    _policyTypeMapper);
            }
            catch (Exception ex)
            {
                _logCreationError(_logger, ex.Message, ex);

                if (ex is CertException)
                {
                    throw;
                }

                throw _exceptionFactory.CreateConnectionException(
                    "Failed to create EWS service",
                    null,
                    context.CorrelationId,
                    ex);
            }
        }

        /// <summary>
        /// Creates a new EWS service with impersonation
        /// </summary>
        public async Task<IEwsServiceBase> CreateServiceWithImpersonationAsync(
            IEwsOperationContext context,
            string impersonatedUser,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrEmpty(impersonatedUser);

            _logCreatingServiceWithImpersonation(_logger, impersonatedUser, context.CorrelationId, null);

            try
            {
                // Create basic service first
                var service = await CreateServiceAsync(context, cancellationToken).ConfigureAwait(false);

                // Configure impersonation using the underlying wrapper
                if (service is EwsServiceImplementation implementationService)
                {
                    await implementationService.ServiceWrapper.ConfigureImpersonationAsync(impersonatedUser, cancellationToken).ConfigureAwait(false);
                }

                return service;
            }
            catch (Exception ex)
            {
                _logCreationError(_logger, ex.Message, ex);

                if (ex is CertException)
                {
                    throw;
                }

                throw _exceptionFactory.CreateConnectionException(
                    $"Failed to create EWS service with impersonation for user {impersonatedUser}",
                    null,
                    context.CorrelationId,
                    ex);
            }
        }

        /// <summary>
        /// Creates the underlying service wrapper
        /// </summary>
        private Task<IExchangeServiceWrapper> CreateServiceWrapperAsync(CancellationToken cancellationToken)
        {
            // Implementation would create the wrapped ExchangeServiceWrapper
            // This would integrate with the existing implementation
            return Task.FromResult<IExchangeServiceWrapper>(null!); // Placeholder
        }

        /// <summary>
        /// Configures the service URL
        /// </summary>
        private Task ConfigureServiceUrlAsync(IExchangeServiceWrapper serviceWrapper, CancellationToken cancellationToken)
        {
            // Implementation would configure the URL from options
            return Task.CompletedTask;
        }

        /// <summary>
        /// Configures authentication for the service
        /// </summary>
        private async Task ConfigureAuthenticationAsync(
            IExchangeServiceWrapper serviceWrapper,
            IEwsOperationContext context,
            CancellationToken cancellationToken)
        {
            // Get authentication options from context or use defaults
            CertAuthenticationOptions authOptions;

            if (context.TryGetPropertyValue<CertAuthenticationOptions>("AuthenticationOptions", out var options) &&
                options != null)
            {
                authOptions = options;
            }
            else
            {
                // Use defaults from configuration
                authOptions = new CertAuthenticationOptions
                {
                    AuthType = _options.AuthType,
                    ClientId = _options.ClientId,
                    TenantId = _options.TenantId,
                    Resource = _options.Resource ?? "https://outlook.office365.com"
                };

                // Store in context for future use
                context.AddProperty("AuthenticationOptions", authOptions);
            }

            // Authenticate
            await _policyExecutor.ExecuteAsync(
                async () => await serviceWrapper.AuthenticateAsync(authOptions, cancellationToken).ConfigureAwait(false),
                EwsPolicyType.Authentication,
                nameof(ConfigureAuthenticationAsync),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Implementation of IEwsServiceBase that uses the Exchange service wrapper
        /// </summary>
        private class EwsServiceImplementation : EwsServiceBase
        {
            /// <summary>
            /// Gets the service wrapper for impersonation configuration
            /// </summary>
            public IExchangeServiceWrapper ServiceWrapper => base.ServiceWrapper;

            /// <summary>
            /// Initializes a new instance of the <see cref="EwsServiceImplementation"/> class
            /// </summary>
            public EwsServiceImplementation(
                IExchangeServiceWrapper serviceWrapper,
                IEwsErrorMappingService errorMappingService,
                IEwsPolicyExecutor policyExecutor,
                ICertExceptionFactory exceptionFactory,
                ILogger logger,
                IEwsPolicyTypeMapper EwsPolicyTypeMapper)
                : base(serviceWrapper, errorMappingService, policyExecutor, exceptionFactory, logger, EwsPolicyTypeMapper)
            {
            }
        }
    }
}
