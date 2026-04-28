// PiQApi.Ews.Service/Core/EwsServiceBase.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Ews.Core.Enums;
using PiQApi.Ews.Core.Interfaces;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Service.Core.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace PiQApi.Ews.Service.Core
{
    /// <summary>
    /// Base implementation for all Exchange Web Services operations
    /// </summary>
    public abstract class EwsServiceBase : IEwsServiceBase
    {
        private readonly IExchangeServiceWrapper _serviceWrapper;
        private readonly IEwsErrorMappingService _errorMappingService;
        private readonly IEwsPolicyExecutor _policyExecutor;
        private readonly ICertExceptionFactory _exceptionFactory;
        private readonly ILogger _logger;
        private readonly IEwsPolicyTypeMapper _policyTypeMapper;

        // LoggerMessage delegates for high-performance logging
        private static readonly Action<ILogger, string, string, Exception?> _logExecuteOperation =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1, "ExecuteAsync"),
                "Executing EWS operation {OperationName}. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> _logOperationSuccess =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(2, "ExecuteAsync"),
                "Successfully executed EWS operation. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, string, string, Exception?> _logOperationFailure =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(3, "ExecuteAsync"),
                "Failed to execute EWS operation: {ErrorMessage}. CorrelationId: {CorrelationId}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceBase"/> class
        /// </summary>
        protected EwsServiceBase(
            IExchangeServiceWrapper serviceWrapper,
            IEwsErrorMappingService errorMappingService,
            IEwsPolicyExecutor policyExecutor,
            ICertExceptionFactory exceptionFactory,
            ILogger logger,
            IEwsPolicyTypeMapper EwsPolicyTypeMapper)
        {
            _serviceWrapper = serviceWrapper ?? throw new ArgumentNullException(nameof(serviceWrapper));
            _errorMappingService = errorMappingService ?? throw new ArgumentNullException(nameof(errorMappingService));
            _policyExecutor = policyExecutor ?? throw new ArgumentNullException(nameof(policyExecutor));
            _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _policyTypeMapper = EwsPolicyTypeMapper ?? throw new ArgumentNullException(nameof(policyTypeMapper));
        }

        /// <summary>
        /// Gets the Exchange service wrapper
        /// </summary>
        protected IExchangeServiceWrapper ServiceWrapper => _serviceWrapper;

        /// <summary>
        /// Gets the policy executor
        /// </summary>
        protected IEwsPolicyExecutor PolicyExecutor => _policyExecutor;

        /// <summary>
        /// Ensures correlation header is set
        /// </summary>
        protected void EnsureCorrelationHeader(string correlationId)
        {
            _serviceWrapper.SetCorrelationId(correlationId);
        }

        /// <summary>
        /// Gets the Exchange service instance
        /// </summary>
        public async Task<ExchangeService> GetServiceAsync(IEwsOperationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            var timer = Stopwatch.StartNew();

            try
            {
                await context.LogOperationStartAsync().ConfigureAwait(false);

                // Set correlation ID on service
                EnsureCorrelationHeader(context.CorrelationId);

                // Ensure service is authenticated
                await EnsureAuthenticatedAsync(context, cancellationToken).ConfigureAwait(false);

                timer.Stop();
                context.Metrics.RecordOperation("GetService", timer.Elapsed, true);

                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return _serviceWrapper.Service;
            }
            catch (Exception ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation("GetService", timer.Elapsed, false);

                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                if (ex is CertException certException)
                {
                    certException.SetCorrelationId(context.CorrelationId);
                }
                else
                {
                    var exception = _exceptionFactory.CreateServiceException(
                        ex.Message,
                        "ServiceAccessError",
                        GetType().Name,
                        nameof(GetServiceAsync),
                        ex);

                    if (exception is CertException wrappedException)
                    {
                        wrappedException.SetCorrelationId(context.CorrelationId);
                    }

                    throw exception;
                }

                throw;
            }
        }

        /// <summary>
        /// Executes an operation with the Exchange service
        /// </summary>
        public async Task<T> ExecuteAsync<T>(
            IEwsOperationContext context,
            Func<ExchangeService, Task<T>> operation,
            Abstractions.Enums.ResiliencePolicyType EwsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            _logExecuteOperation(_logger, operationName, context.CorrelationId, null);

            var timer = Stopwatch.StartNew();

            try
            {
                await context.LogOperationStartAsync().ConfigureAwait(false);

                // Ensure correlation ID is set
                EnsureCorrelationHeader(context.CorrelationId);

                // Execute the operation with policy
                var result = await _policyExecutor.ExecuteAsync(
                    async () =>
                    {
                        var service = await GetServiceAsync(context, cancellationToken).ConfigureAwait(false);
                        return await operation(service).ConfigureAwait(false);
                    },
                    EwsPolicyType,
                    operationName,
                    cancellationToken).ConfigureAwait(false);

                timer.Stop();
                context.Metrics.RecordOperation(operationName, timer.Elapsed, true);

                _logOperationSuccess(_logger, context.CorrelationId, null);
                await context.LogOperationEndAsync(true).ConfigureAwait(false);

                return result;
            }
            catch (ServiceResponseException ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation(operationName, timer.Elapsed, false);

                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                _logOperationFailure(_logger, ex.Message, context.CorrelationId, ex);

                var mappedException = _errorMappingService.MapServiceException(ex, context.CorrelationId);

                if (mappedException is CertException certException)
                {
                    certException.SetCorrelationId(context.CorrelationId);
                }

                throw mappedException;
            }
            catch (Exception ex) when (ex is not CertException)
            {
                timer.Stop();
                context.Metrics.RecordOperation(operationName, timer.Elapsed, false);

                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                _logOperationFailure(_logger, ex.Message, context.CorrelationId, ex);

                var exception = _exceptionFactory.CreateServiceException(
                    ex.Message,
                    "ExchangeServiceError",
                    GetType().Name,
                    operationName,
                    ex);

                if (exception is CertException certException)
                {
                    certException.SetCorrelationId(context.CorrelationId);
                }

                throw exception;
            }
        }

        /// <summary>
        /// Executes an operation with the Exchange service without a return value
        /// </summary>
        public async Task ExecuteAsync(
            IEwsOperationContext context,
            Func<ExchangeService, Task> operation,
            Abstractions.Enums.ResiliencePolicyType EwsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            _logExecuteOperation(_logger, operationName, context.CorrelationId, null);

            var timer = Stopwatch.StartNew();

            try
            {
                await context.LogOperationStartAsync().ConfigureAwait(false);

                // Ensure correlation ID is set
                EnsureCorrelationHeader(context.CorrelationId);

                // Execute the operation with policy
                await _policyExecutor.ExecuteAsync(
                    async () =>
                    {
                        var service = await GetServiceAsync(context, cancellationToken).ConfigureAwait(false);
                        await operation(service).ConfigureAwait(false);
                    },
                    EwsPolicyType,
                    operationName,
                    cancellationToken).ConfigureAwait(false);

                timer.Stop();
                context.Metrics.RecordOperation(operationName, timer.Elapsed, true);

                _logOperationSuccess(_logger, context.CorrelationId, null);
                await context.LogOperationEndAsync(true).ConfigureAwait(false);
            }
            catch (ServiceResponseException ex)
            {
                timer.Stop();
                context.Metrics.RecordOperation(operationName, timer.Elapsed, false);

                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                _logOperationFailure(_logger, ex.Message, context.CorrelationId, ex);

                var mappedException = _errorMappingService.MapServiceException(ex, context.CorrelationId);

                if (mappedException is CertException certException)
                {
                    certException.SetCorrelationId(context.CorrelationId);
                }

                throw mappedException;
            }
            catch (Exception ex) when (ex is not CertException)
            {
                timer.Stop();
                context.Metrics.RecordOperation(operationName, timer.Elapsed, false);

                await context.LogOperationErrorAsync(ex).ConfigureAwait(false);

                _logOperationFailure(_logger, ex.Message, context.CorrelationId, ex);

                var exception = _exceptionFactory.CreateServiceException(
                    ex.Message,
                    "ExchangeServiceError",
                    GetType().Name,
                    operationName,
                    ex);

                if (exception is CertException certException)
                {
                    certException.SetCorrelationId(context.CorrelationId);
                }

                throw exception;
            }
        }

        /// <summary>
        /// Helper method for executing an Exchange-specific operation with the appropriate policy
        /// </summary>
        protected async Task<T> ExecuteWithEwsPolicyAsync<T>(
            IEwsOperationContext context,
            Func<ExchangeService, Task<T>> operation,
            EwsPolicyType ewsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            var corePolicyType = _policyTypeMapper.MapToCorePolicy(ewsPolicyType);
            return await ExecuteAsync(context, operation, corePolicyType, operationName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper method for executing an Exchange-specific operation with the appropriate policy without a return value
        /// </summary>
        protected async Task ExecuteWithEwsPolicyAsync(
            IEwsOperationContext context,
            Func<ExchangeService, Task> operation,
            EwsPolicyType ewsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            var corePolicyType = _policyTypeMapper.MapToCorePolicy(ewsPolicyType);
            await ExecuteAsync(context, operation, corePolicyType, operationName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Ensures the service is authenticated
        /// </summary>
        public virtual async Task EnsureAuthenticatedAsync(IEwsOperationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (_serviceWrapper.AuthenticationStatus != Abstractions.Enums.AuthenticationStatusType.Authenticated)
            {
                _logger.LogDebug("Service not authenticated, attempting authentication. CorrelationId: {CorrelationId}",
                    context.CorrelationId);

                // Retrieve authentication options from context properties
                if (context.TryGetPropertyValue<PiQApi.Core.Authentication.CertAuthenticationOptions>("AuthenticationOptions", out var options) && options != null)
                {
                    await ExecuteWithEwsPolicyAsync(
                        context,
                        async _ => await _serviceWrapper.AuthenticateAsync(options, cancellationToken).ConfigureAwait(false),
                        EwsPolicyType.Authentication,
                        nameof(EnsureAuthenticatedAsync),
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var exception = _exceptionFactory.CreateAuthenticationException(
                        "No authentication options available in context",
                        "MissingAuthenticationOptions");

                    exception.SetCorrelationId(context.CorrelationId);
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Sets the correlation ID for the service
        /// </summary>
        public void SetCorrelationId(string correlationId)
        {
            ArgumentException.ThrowIfNullOrEmpty(correlationId);
            _serviceWrapper.SetCorrelationId(correlationId);
        }
    }
}
