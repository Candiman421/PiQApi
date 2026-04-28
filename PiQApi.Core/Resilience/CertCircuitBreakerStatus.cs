// PiQApi.Core/Resilience/CertCircuitBreakerStatus.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Implements the circuit breaker pattern to prevent repeated failed operations
/// </summary>
public class CertCircuitBreakerStatus : IAsyncDisposable
{
    private readonly ICertAsyncLock _lock;
    private readonly double _failureThreshold;
    private readonly TimeSpan _breakDuration;
    private readonly ICertTimeProvider _timeProvider;
    private readonly ICertCorrelationContext? _correlationContext;
    private readonly ILogger<CertCircuitBreakerStatus>? _logger;
    private int _totalRequests;
    private int _failedRequests;
    private DateTime _lastFailure = DateTime.MinValue;
    private bool _isOpen;
    private bool _isDisposed;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, bool, Exception?> LogCircuitStateChange =
        LoggerMessage.Define<string, bool>(
            LogLevel.Information,
            new EventId(1, "CircuitStateChange"),
            "[{CorrelationId}] Circuit breaker state changed to {IsOpen}");

    private static readonly Action<ILogger, string, double, Exception?> LogFailureRateUpdate =
        LoggerMessage.Define<string, double>(
            LogLevel.Debug,
            new EventId(2, "FailureRateUpdate"),
            "[{CorrelationId}] Circuit breaker failure rate: {FailureRate}");

    private static readonly Action<ILogger, string, Exception?> LogCircuitReset =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3, "CircuitReset"),
            "[{CorrelationId}] Circuit breaker has been reset");

    /// <summary>
    /// Gets whether the circuit is open
    /// </summary>
    public bool IsOpen
    {
        get
        {
            var isOpen = _isOpen;

            // Check if the break duration has elapsed
            if (isOpen && _timeProvider.UtcNow >= _lastFailure.Add(_breakDuration))
            {
                // We'll try to close the circuit conditionally when queried
                // This is done without a lock since we'll do a proper check in ShouldRemainOpen()
                isOpen = false;
            }

            return isOpen;
        }
    }

    /// <summary>
    /// Gets the time when the circuit will reset
    /// </summary>
    public DateTime ResetTime => _lastFailure.Add(_breakDuration);

    /// <summary>
    /// Gets the failure rate
    /// </summary>
    public double FailureRate => _totalRequests == 0 ? 0 : (double)_failedRequests / _totalRequests;

    /// <summary>
    /// Gets the total number of requests
    /// </summary>
    public int TotalRequests => _totalRequests;

    /// <summary>
    /// Gets the number of failed requests
    /// </summary>
    public int FailedRequests => _failedRequests;

    /// <summary>
    /// Initializes a new instance of the CertCircuitBreakerStatus class
    /// </summary>
    /// <param name="options">Circuit breaker configuration options</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    /// <param name="asyncLockFactory">Async lock factory</param>
    /// <param name="correlationContext">Correlation context for tracing</param>
    /// <param name="logger">Logger</param>
    public CertCircuitBreakerStatus(
        CertCircuitBreakerOptions options,
        ICertTimeProvider timeProvider,
        ICertAsyncLockFactory asyncLockFactory,
        ICertCorrelationContext? correlationContext = null,
        ILogger<CertCircuitBreakerStatus>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(asyncLockFactory);

        _failureThreshold = options.FailureThreshold;
        _breakDuration = options.DurationOfBreak;
        _timeProvider = timeProvider;
        _correlationContext = correlationContext;
        _logger = logger;
        _lock = asyncLockFactory.Create();
    }

    /// <summary>
    /// Initializes a new instance of the CertCircuitBreakerStatus class with explicit parameters
    /// </summary>
    /// <param name="failureThreshold">Threshold at which the circuit should open</param>
    /// <param name="breakDuration">How long the circuit should stay open</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    /// <param name="asyncLockFactory">Async lock factory</param>
    /// <param name="correlationContext">Correlation context for tracing</param>
    /// <param name="logger">Logger</param>
    public CertCircuitBreakerStatus(
        double failureThreshold,
        TimeSpan breakDuration,
        ICertTimeProvider timeProvider,
        ICertAsyncLockFactory asyncLockFactory,
        ICertCorrelationContext? correlationContext = null,
        ILogger<CertCircuitBreakerStatus>? logger = null)
    {
        if (failureThreshold is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be between 0 and 1");
        }

        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(asyncLockFactory);

        _failureThreshold = failureThreshold;
        _breakDuration = breakDuration;
        _timeProvider = timeProvider;
        _correlationContext = correlationContext;
        _logger = logger;
        _lock = asyncLockFactory.Create();
    }

    /// <summary>
    /// Records a success and updates the circuit state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RecordSuccessAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("CircuitBreakerOperation", "RecordSuccess");

        using var lockReleaser = await _lock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        _totalRequests++;
        if (_totalRequests > 100)
        {
            // Reset counters periodically to avoid overflow and give recent history more weight
            _totalRequests = 100;
            _failedRequests = (int)(_failedRequests * 0.9); // Decay older failures
        }

        // Log the updated failure rate
        if (_logger != null && _correlationContext != null)
        {
            LogFailureRateUpdate(_logger, _correlationContext.CorrelationId, FailureRate, null);
        }
    }

    /// <summary>
    /// Records a failure and updates the circuit state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RecordFailureAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("CircuitBreakerOperation", "RecordFailure");

        using var lockReleaser = await _lock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        _totalRequests++;
        _failedRequests++;
        _lastFailure = _timeProvider.UtcNow;

        bool previousState = _isOpen;

        // Check if we should open the circuit
        if (FailureRate >= _failureThreshold && _totalRequests >= 5)
        {
            _isOpen = true;
        }

        if (_totalRequests > 100)
        {
            // Reset counters periodically to avoid overflow and give recent history more weight
            _totalRequests = 100;
            _failedRequests = (int)(_failedRequests * 0.9); // Decay older failures
        }

        // Log state change if it occurred
        if (_logger != null && _correlationContext != null)
        {
            // Log failure rate for monitoring
            LogFailureRateUpdate(_logger, _correlationContext.CorrelationId, FailureRate, null);

            // Log circuit state change if it occurred
            if (previousState != _isOpen)
            {
                LogCircuitStateChange(_logger, _correlationContext.CorrelationId, _isOpen, null);
            }
        }
    }

    /// <summary>
    /// Checks if the circuit should remain open
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the circuit should remain open</returns>
    public async Task<bool> ShouldRemainOpenAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("CircuitBreakerOperation", "ShouldRemainOpen");

        using var lockReleaser = await _lock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        // If the circuit is closed, it should remain closed
        if (!_isOpen)
        {
            return false;
        }

        // If we're still in the break duration, keep it open
        bool shouldRemainOpen = _timeProvider.UtcNow < _lastFailure.Add(_breakDuration);

        // If the break duration has elapsed, close the circuit
        if (!shouldRemainOpen)
        {
            _isOpen = false;

            // Log the state change
            if (_logger != null && _correlationContext != null)
            {
                LogCircuitStateChange(_logger, _correlationContext.CorrelationId, _isOpen, null);
            }
        }

        return shouldRemainOpen;
    }

    /// <summary>
    /// Closes the circuit
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("CircuitBreakerOperation", "Close");

        using var lockReleaser = await _lock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        bool previousState = _isOpen;
        _isOpen = false;

        // Log state change if it occurred
        if (previousState && _logger != null && _correlationContext != null)
        {
            LogCircuitStateChange(_logger, _correlationContext.CorrelationId, _isOpen, null);
        }
    }

    /// <summary>
    /// Resets the circuit breaker state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("CircuitBreakerOperation", "Reset");

        using var lockReleaser = await _lock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        _isOpen = false;
        _totalRequests = 0;
        _failedRequests = 0;
        _lastFailure = DateTime.MinValue;

        // Log the reset
        if (_logger != null && _correlationContext != null)
        {
            LogCircuitReset(_logger, _correlationContext.CorrelationId, null);
        }
    }

    /// <summary>
    /// Disposes the resources used by the circuit breaker
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await _lock.DisposeAsync().ConfigureAwait(false);

        // Suppress finalization to address CA1816
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Backwards compatibility non-async methods
    /// </summary>

    /// <summary>
    /// Records a success and updates the circuit state (synchronous version)
    /// </summary>
    public void RecordSuccess()
    {
        Task.Run(() => RecordSuccessAsync()).Wait();
    }

    /// <summary>
    /// Records a failure and updates the circuit state (synchronous version)
    /// </summary>
    public void RecordFailure()
    {
        Task.Run(() => RecordFailureAsync()).Wait();
    }

    /// <summary>
    /// Checks if the circuit should remain open (synchronous version)
    /// </summary>
    /// <returns>True if the circuit should remain open</returns>
    public bool ShouldRemainOpen()
    {
        return Task.Run(() => ShouldRemainOpenAsync()).Result;
    }

    /// <summary>
    /// Closes the circuit (synchronous version)
    /// </summary>
    public void Close()
    {
        Task.Run(() => CloseAsync()).Wait();
    }

    /// <summary>
    /// Resets the circuit breaker state (synchronous version)
    /// </summary>
    public void Reset()
    {
        Task.Run(() => ResetAsync()).Wait();
    }
}
