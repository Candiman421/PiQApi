// CertApi.Ews.Service/Core/EwsPolicyExecutor.cs
using CertApi.Abstractions.Resilience;
using CertApi.Ews.Core.Enums;
using CertApi.Ews.Core.Interfaces;
using CertApi.Ews.Service.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CertApi.Ews.Service.Core
{
    /// <summary>
    /// Implementation of policy executor for Exchange service operations
    /// </summary>
    public class EwsPolicyExecutor : IEwsPolicyExecutor
    {
        private readonly ICertResiliencePolicyExecutor _corePolicyExecutor;
        private readonly ILogger<EwsPolicyExecutor> _logger;
        private readonly IEwsPolicyTypeMapper _policyTypeMapper;

        // LoggerMessage delegates for high-performance logging
        private static readonly Action<ILogger, string, EwsPolicyType, Exception?> _logExecutingOperation =
            LoggerMessage.Define<string, EwsPolicyType>(
                LogLevel.Debug,
                new EventId(1, "ExecuteAsync"),
                "Executing operation {OperationName} with policy type {PolicyType}");

        private static readonly Action<ILogger, string, EwsPolicyType, Exception> _logExecutionError =
            LoggerMessage.Define<string, EwsPolicyType>(
                LogLevel.Error,
                new EventId(2, "ExecuteAsync"),
                "Error executing operation {OperationName} with policy type {PolicyType}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsPolicyExecutor"/> class
        /// </summary>
        /// <param name="corePolicyExecutor">Core resilience policy executor</param>
        /// <param name="logger">Logger</param>
        /// <param name="policyTypeMapper">Policy type mapper</param>
        public EwsPolicyExecutor(
            ICertResiliencePolicyExecutor corePolicyExecutor,
            ILogger<EwsPolicyExecutor> logger,
            IEwsPolicyTypeMapper EwsPolicyTypeMapper)
        {
            _corePolicyExecutor = corePolicyExecutor ?? throw new ArgumentNullException(nameof(corePolicyExecutor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _policyTypeMapper = EwsPolicyTypeMapper ?? throw new ArgumentNullException(nameof(policyTypeMapper));
        }

        /// <summary>
        /// Executes an operation with the specified policy type
        /// </summary>
        public async Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation,
            EwsPolicyType EwsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            _logExecutingOperation(_logger, operationName, EwsPolicyType, null);

            // Map our policy type to core resilience policy type
            var corePolicyType = _policyTypeMapper.MapToCorePolicy(policyType);

            try
            {
                // Execute the operation using the core resilience policy executor
                return await _corePolicyExecutor.ExecuteAsync(
                    operation,
                    corePolicyType,
                    operationName,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logExecutionError(_logger, operationName, EwsPolicyType, ex);
                throw;
            }
        }

        /// <summary>
        /// Executes an operation with the specified policy type without a return value
        /// </summary>
        public async Task ExecuteAsync(
            Func<Task> operation,
            EwsPolicyType EwsPolicyType,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            _logExecutingOperation(_logger, operationName, EwsPolicyType, null);

            // Map our policy type to core resilience policy type
            var corePolicyType = _policyTypeMapper.MapToCorePolicy(policyType);

            try
            {
                // Execute the operation using the core resilience policy executor
                await _corePolicyExecutor.ExecuteAsync(
                    operation,
                    corePolicyType,
                    operationName,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logExecutionError(_logger, operationName, EwsPolicyType, ex);
                throw;
            }
        }
    }
}
