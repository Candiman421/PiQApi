// PiQApi.Ews.Service/Core/EwsConnectionManager.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Exceptions.Base;
using PiQApi.Ews.Core.Context;
using PiQApi.Ews.Core.Interfaces;
using PiQApi.Ews.Core.Interfaces.Context;
using PiQApi.Ews.Core.Models;
using PiQApi.Ews.Service.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace PiQApi.Ews.Service.Core
{
    /// <summary>
    /// Manages EWS service connections with pooling and resilience
    /// </summary>
    public class EwsConnectionManager : IEwsConnectionManager
    {
        private readonly ILogger<EwsConnectionManager> _logger;
        private readonly IEwsServiceFactory _serviceFactory;
        private readonly IPiQExceptionFactory _exceptionFactory;
        private readonly IPiQAsyncLockFactory _lockFactory;
        private readonly EwsConnectionOptions _options;

        private readonly ConcurrentBag<IEwsServiceBase> _servicePool;
        private readonly SemaphoreSlim _poolSemaphore;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly object _circuitBreakerLock = new();
        private readonly IPiQAsyncLock _stateLock;

        private int _failureCount;
        private int _successCount;
        private DateTimeOffset _circuitOpenTime;
        private bool _isCircuitOpen;

        // LoggerMessage delegates
        private static readonly Action<ILogger, Exception?> _initializingLog =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1, nameof(InitializeAsync)),
                "Initializing EWS connection manager");

        private static readonly Action<ILogger, ConnectionStateType, ConnectionStateType, Exception?> _stateChangedLog =
            LoggerMessage.Define<ConnectionStateType, ConnectionStateType>(
                LogLevel.Information,
                new EventId(2, "UpdateConnectionState"),
                "Connection state changed from {OldState} to {NewState}");

        private static readonly Action<ILogger, string, Exception?> _acquiringServiceLog =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(3, nameof(AcquireServiceAsync)),
                "Acquiring EWS service. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, double, string, Exception?> _serviceAcquiredLog =
            LoggerMessage.Define<double, string>(
                LogLevel.Debug,
                new EventId(4, nameof(AcquireServiceAsync)),
                "Acquired EWS service in {ElapsedMs}ms. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> _releasingServiceLog =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(5, nameof(ReleaseServiceAsync)),
                "Releasing EWS service. CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, Exception?> _circuitBreakerOpenLog =
            LoggerMessage.Define(
                LogLevel.Warning,
                new EventId(6, nameof(AcquireServiceAsync)),
                "Circuit breaker is open, service acquisition denied");

        private static readonly Action<ILogger, Exception?> _circuitBreakerClosedLog =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(7, nameof(AcquireServiceAsync)),
                "Circuit breaker closed, allowing connection attempt");

        private static readonly Action<ILogger, string, Exception> _connectionErrorLog =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(8, "OnConnectionError"),
                "Connection error: {ErrorMessage}");

        private static readonly Action<ILogger, Exception?> _disposingLog =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(9, "DisposeAsync"),
                "Disposing EWS connection manager");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsConnectionManager"/> class
        /// </summary>
        public EwsConnectionManager(
            ILogger<EwsConnectionManager> logger,
            IEwsServiceFactory serviceFactory,
            IPiQExceptionFactory exceptionFactory,
            IPiQAsyncLockFactory lockFactory,
            IOptions<EwsConnectionOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            _lockFactory = lockFactory ?? throw new ArgumentNullException(nameof(lockFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _servicePool = new ConcurrentBag<IEwsServiceBase>();
            _poolSemaphore = new SemaphoreSlim(_options.MaxPoolSize);
            _cancellationTokenSource = new CancellationTokenSource();
            _stateLock = _lockFactory.Create();

            State = ConnectionStateType.Disconnected;
            LastConnectedTime = DateTimeOffset.MinValue;

            // Initialize events
            ConnectionStateChanged = (_, _) => { };
            ConnectionError = (_, _) => { };
        }

        /// <summary>
        /// Gets the current connection state
        /// </summary>
        public ConnectionStateType State { get; private set; }

        /// <summary>
        /// Gets the last time a connection was established
        /// </summary>
        public DateTimeOffset LastConnectedTime { get; private set; }

        /// <summary>
        /// Gets the current number of connections in the pool
        /// </summary>
        public int CurrentPoolSize => _servicePool.Count;

        /// <summary>
        /// Gets whether circuit breaker is open
        /// </summary>
        public bool IsCircuitBreakerOpen => _isCircuitOpen;

        /// <summary>
        /// Event raised when connection state changes
        /// </summary>
        public event EventHandler<EwsConnectionEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Event raised when a connection error occurs
        /// </summary>
        public event EventHandler<EwsConnectionEventArgs> ConnectionError;

        /// <summary>
        /// Initializes the connection manager
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _initializingLog(_logger, null);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _cancellationTokenSource.Token);

            try
            {
                // Create a context for initialization
                using var initLock = await _stateLock.AcquireAsync(linkedCts.Token).ConfigureAwait(false);

                // Warm up the pool
                await WarmupPoolAsync(_options.MinPoolSize, linkedCts.Token).ConfigureAwait(false);

                // Update state
                await UpdateConnectionStateAsync(ConnectionStateType.Connected).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                await UpdateConnectionStateAsync(ConnectionStateType.Error).ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                await UpdateConnectionStateAsync(ConnectionStateType.Error).ConfigureAwait(false);

                throw _exceptionFactory.CreateConnectionException(
                    "Failed to initialize EWS connection manager",
                    null,
                    Guid.NewGuid().ToString(),
                    ex);
            }
        }

        /// <summary>
        /// Acquires a service from the pool
        /// </summary>
        public async Task<IEwsServiceBase> AcquireServiceAsync(IEwsOperationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            _acquiringServiceLog(_logger, context.CorrelationId, null);
            var startTime = DateTimeOffset.UtcNow;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _cancellationTokenSource.Token);

            try
            {
                // Check circuit breaker
                if (_options.EnableCircuitBreaker && _isCircuitOpen)
                {
                    // Check if timeout has elapsed
                    if (DateTimeOffset.UtcNow - _circuitOpenTime > _options.CircuitBreakerDuration)
                    {
                        lock (_circuitBreakerLock)
                        {
                            if (_isCircuitOpen && DateTimeOffset.UtcNow - _circuitOpenTime > _options.CircuitBreakerDuration)
                            {
                                _isCircuitOpen = false;
                                _circuitBreakerClosedLog(_logger, null);
                            }
                        }
                    }
                    else
                    {
                        _circuitBreakerOpenLog(_logger, null);
                        throw new PiQCircuitBreakerOpenException(
                            "Circuit breaker is open, connection acquisition denied",
                            "EwsConnectionManager");
                    }
                }

                // Wait for semaphore
                await _poolSemaphore.WaitAsync(linkedCts.Token).ConfigureAwait(false);

                try
                {
                    // Try to get service from pool
                    if (_options.EnablePooling && _servicePool.TryTake(out var service))
                    {
                        // Validate if required
                        if (_options.ValidateOnAcquire)
                        {
                            var isValid = await ValidateServiceAsync(service, context, linkedCts.Token).ConfigureAwait(false);
                            if (isValid)
                            {
                                var elapsed = (DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
                                _serviceAcquiredLog(_logger, elapsed, context.CorrelationId, null);

                                // Record success
                                RecordSuccess();

                                context.Metrics.RecordOperation("AcquireService", TimeSpan.FromMilliseconds(elapsed), true);
                                return service;
                            }
                        }
                        else
                        {
                            // No validation required
                            var elapsed = (DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
                            _serviceAcquiredLog(_logger, elapsed, context.CorrelationId, null);

                            // Record success
                            RecordSuccess();

                            context.Metrics.RecordOperation("AcquireService", TimeSpan.FromMilliseconds(elapsed), true);
                            return service;
                        }
                    }

                    // Create new service
                    var newService = await _serviceFactory.CreateServiceAsync(context, linkedCts.Token).ConfigureAwait(false);

                    // Update timestamps
                    LastConnectedTime = DateTimeOffset.UtcNow;

                    var elapsed = (DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
                    _serviceAcquiredLog(_logger, elapsed, context.CorrelationId, null);

                    // Record success
                    RecordSuccess();

                    context.Metrics.RecordOperation("AcquireService", TimeSpan.FromMilliseconds(elapsed), true);
                    return newService;
                }
                catch (Exception)
                {
                    // Release semaphore on error
                    _poolSemaphore.Release();
                    throw;
                }
            }
            catch (Exception ex)
            {
                var elapsed = (DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
                context.Metrics.RecordOperation("AcquireService", TimeSpan.FromMilliseconds(elapsed), false);

                // Record failure in circuit breaker
                RecordFailure();

                // Raise error event
                OnConnectionError(ex, context.CorrelationId);

                if (ex is PiQCircuitBreakerOpenException || ex is PiQException)
                {
                    throw;
                }

                throw _exceptionFactory.CreateConnectionException(
                    "Failed to acquire EWS service",
                    null,
                    context.CorrelationId,
                    ex);
            }
        }

        /// <summary>
        /// Releases a service back to the pool
        /// </summary>
        public async Task ReleaseServiceAsync(IEwsServiceBase service, IEwsOperationContext context)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(context);

            _releasingServiceLog(_logger, context.CorrelationId, null);

            try
            {
                bool isValid = true;

                // Validate if required
                if (_options.ValidateOnRelease)
                {
                    isValid = await ValidateServiceAsync(service, context).ConfigureAwait(false);
                }

                // Return to pool if valid and pooling enabled
                if (isValid && _options.EnablePooling)
                {
                    _servicePool.Add(service);
                }

                context.Metrics.RecordOperation("ReleaseService", TimeSpan.Zero, true);
            }
            finally
            {
                // Always release semaphore
                _poolSemaphore.Release();
            }
        }

        /// <summary>
        /// Validates a service
        /// </summary>
        public async Task<bool> ValidateServiceAsync(IEwsServiceBase service, IEwsOperationContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                // Try to get service without actually using it - this will validate authentication
                _ = await service.GetServiceAsync(context, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Resets the circuit breaker
        /// </summary>
        public Task ResetCircuitBreakerAsync()
        {
            lock (_circuitBreakerLock)
            {
                _isCircuitOpen = false;
                _failureCount = 0;
                _successCount = 0;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Warms up the connection pool
        /// </summary>
        private async Task WarmupPoolAsync(int count, CancellationToken cancellationToken)
        {
            if (count <= 0)
                return;

            if (!_options.EnablePooling)
                return;

            // Create a temporary context for warmup
            var context = new EwsOperationContext(
                await Task.FromResult<IPiQOperationContext>(null!),
                await Task.FromResult<IEwsCorrelationContext>(null!),
                _logger);

            // Create services
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var service = await _serviceFactory.CreateServiceAsync(context, cancellationToken).ConfigureAwait(false);
                    _servicePool.Add(service);
                }
                catch (Exception)
                {
                    // Continue with next service
                }
            }
        }

        /// <summary>
        /// Updates the connection state and raises the state changed event
        /// </summary>
        private async Task UpdateConnectionStateAsync(ConnectionStateType newState)
        {
            using var stateLock = await _stateLock.AcquireAsync().ConfigureAwait(false);

            if (State != newState)
            {
                var oldState = State;
                State = newState;

                _stateChangedLog(_logger, oldState, newState, null);

                var args = new EwsConnectionEventArgs
                {
                    OldState = oldState,
                    NewState = newState
                };

                ConnectionStateChanged?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Records a connection success
        /// </summary>
        private void RecordSuccess()
        {
            if (!_options.EnableCircuitBreaker)
                return;

            lock (_circuitBreakerLock)
            {
                _successCount++;
            }
        }

        /// <summary>
        /// Records a connection failure
        /// </summary>
        private void RecordFailure()
        {
            if (!_options.EnableCircuitBreaker)
                return;

            lock (_circuitBreakerLock)
            {
                _failureCount++;

                // Check if we need to open the circuit
                if (!_isCircuitOpen)
                {
                    int total = _failureCount + _successCount;
                    if (total >= 10 && (double)_failureCount / total >= _options.CircuitBreakerThreshold)
                    {
                        _isCircuitOpen = true;
                        _circuitOpenTime = DateTimeOffset.UtcNow;

                        // Reset counters
                        _failureCount = 0;
                        _successCount = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the connection error event
        /// </summary>
        private void OnConnectionError(Exception exception, string correlationId)
        {
            _connectionErrorLog(_logger, exception.Message, exception);

            var args = new EwsConnectionEventArgs
            {
                Exception = exception,
                NewState = ConnectionStateType.Error,
                CorrelationId = correlationId
            };

            ConnectionError?.Invoke(this, args);
        }

        /// <summary>
        /// Disposes the connection manager
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _disposingLog(_logger, null);

            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);

            while (_servicePool.TryTake(out var service))
            {
                // Services might be IAsyncDisposable
                if (service is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _poolSemaphore.Dispose();
            _cancellationTokenSource.Dispose();

            if (_stateLock is IAsyncDisposable asyncLock)
            {
                await asyncLock.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
