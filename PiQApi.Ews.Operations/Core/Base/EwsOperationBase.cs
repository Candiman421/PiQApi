// PiQApi.Ews.Operations/Core/Base/EwsOperationBase.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Operations;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Core.Operations;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Operations.Core.Interfaces;
using PiQApi.Ews.Service.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Ews.Operations.Core.Base
{
    /// <summary>
    /// Base class for all Exchange operations
    /// </summary>
    public abstract class EwsOperationBase : IEwsOperationBase
    {
        private readonly object _stateLock = new();
        private bool _disposed;
        private bool _initialized;
        private readonly CertOperationBase _baseOperation;

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, Exception?> LogDisposeError =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(100, nameof(DisposeAsync)),
                "Error logging operation end during disposal. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> LogOperationalCheckFailure =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(101, nameof(IsOperationalAsync)),
                "Operation is not operational: {ErrorMessage}");
            
        private static readonly Action<ILogger, string, Exception?> LogInitialization =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(102, nameof(InitializeAsync)),
                "Initializing EWS operation {OperationId}");

        private static readonly Action<ILogger, string, Exception?> LogStateValidation =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(103, nameof(ValidateStateAsync)),
                "Validating EWS operation state {OperationId}");

        /// <summary>
        /// Gets the logger instance
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the exception factory
        /// </summary>
        protected ICertExceptionFactory ExceptionFactory { get; }

        /// <summary>
        /// Gets the validation processor
        /// </summary>
        protected ICertValidationProcessor ValidationProcessor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsOperationBase"/> class
        /// </summary>
        /// <param name="context">Exchange operation context</param>
        /// <param name="serviceWrapper">Exchange service wrapper</param>
        /// <param name="logger">Logger</param>
        /// <param name="exceptionFactory">Exception factory</param>
        /// <param name="validationProcessor">Validation processor</param>
        /// <param name="baseOperation">Core operation base (for composition approach)</param>
        protected EwsOperationBase(
            IEwsOperationContext context,
            IExchangeServiceWrapper serviceWrapper,
            ILogger logger,
            ICertExceptionFactory exceptionFactory,
            ICertValidationProcessor validationProcessor,
            CertOperationBase baseOperation)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ServiceWrapper = serviceWrapper ?? throw new ArgumentNullException(nameof(serviceWrapper));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ExceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            ValidationProcessor = validationProcessor ?? throw new ArgumentNullException(nameof(validationProcessor));
            _baseOperation = baseOperation ?? throw new ArgumentNullException(nameof(baseOperation));
        }

        /// <summary>
        /// Gets the operation ID
        /// </summary>
        public string OperationId => Context.OperationId;

        /// <summary>
        /// Gets the correlation ID
        /// </summary>
        public string CorrelationId => Context.CorrelationId;

        /// <summary>
        /// Gets whether the operation is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                lock (_stateLock)
                {
                    return _initialized && !_disposed;
                }
            }
        }

        /// <summary>
        /// Gets the Exchange operation context
        /// </summary>
        public IEwsOperationContext Context { get; }

        /// <summary>
        /// Gets the Exchange service wrapper
        /// </summary>
        public IExchangeServiceWrapper ServiceWrapper { get; }

        /// <summary>
        /// Initializes the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            lock (_stateLock)
            {
                if (_initialized)
                {
                    throw CreateExceptionWithCorrelation(
                        "Operation already initialized",
                        "AlreadyInitialized",
                        nameof(InitializeAsync));
                }
                _initialized = true;
            }

            LogInitialization(Logger, OperationId, null);

            // Also initialize the base operation
            await _baseOperation.InitializeAsync(cancellationToken).ConfigureAwait(false);

            // Start operation tracking
            await Context.LogOperationStartAsync().ConfigureAwait(false);

            // Perform any additional initialization
            await OnInitializedAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called after initialization is complete
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task OnInitializedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Validates the operation state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ValidateStateAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            LogStateValidation(Logger, OperationId, null);

            if (!IsReady)
            {
                throw CreateExceptionWithCorrelation(
                    "Operation not initialized",
                    "NotInitialized",
                    nameof(ValidateStateAsync));
            }

            // Ensure the service wrapper is also in a valid state
            if (ServiceWrapper.AuthenticationStatus != AuthenticationStatusType.Authenticated)
            {
                throw CreateExceptionWithCorrelation(
                    "Exchange service is not authenticated",
                    "NotAuthenticated",
                    nameof(ValidateStateAsync));
            }

            // Perform any additional validation
            await OnValidateStateAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called during state validation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task OnValidateStateAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Checks if the operation is operational
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", 
             Justification = "Method intentionally catches all exceptions to report operational status")]
        public virtual async Task<bool> IsOperationalAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateStateAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                LogOperationalCheckFailure(Logger, ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Cleans up resources used by the operation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task CleanupAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                // Log the operation end
                await Context.LogOperationEndAsync(false).ConfigureAwait(false);
                
                // Additional cleanup logic
                await OnCleanupAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error during cleanup for operation {OperationId}", OperationId);
            }
        }

        /// <summary>
        /// Called during cleanup
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task OnCleanupAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Creates an exception with correlation ID
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="operationName">Operation name</param>
        /// <returns>An exception with correlation ID</returns>
        protected Exception CreateExceptionWithCorrelation(string message, string errorCode, string operationName)
        {
            var exception = ExceptionFactory.CreateServiceException(
                message,
                errorCode,
                nameof(EwsOperationBase),
                operationName);

            if (exception is CertException certException)
            {
                certException.SetCorrelationId(CorrelationId);
            }

            return exception;
        }

        /// <summary>
        /// Throws if the operation is disposed
        /// </summary>
        /// <param name="memberName">Caller member name</param>
        /// <exception cref="ObjectDisposedException">Thrown if the operation is disposed</exception>
        protected void ThrowIfDisposed([System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
        {
            if (_disposed)
            {
                var exception = new ObjectDisposedException(GetType().Name, $"Cannot access the operation in {memberName} after it has been disposed.");
                
                // Add correlation ID to the exception data
                exception.Data["CorrelationId"] = CorrelationId;
                
                throw exception;
            }
        }

        /// <summary>
        /// Disposes the operation
        /// </summary>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", 
            Justification = "Disposal methods should never throw exceptions")]
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            lock (_stateLock)
            {
                if (_disposed) return;
                _disposed = true;
            }

            try
            {
                // Log operation end if it was initialized
                if (_initialized)
                {
                    await Context.LogOperationEndAsync(false).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // Just log the exception, don't throw during disposal
                LogDisposeError(Logger, CorrelationId, ex);
            }

            // Dispose the base operation
            await ((IAsyncDisposable)_baseOperation).DisposeAsync().ConfigureAwait(false);

            // Perform any additional cleanup
            await DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs additional cleanup during disposal
        /// </summary>
        protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;
    }
}