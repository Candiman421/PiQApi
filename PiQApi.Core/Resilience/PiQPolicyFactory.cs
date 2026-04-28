// PiQApi.Core/Resilience/PiQPolicyFactory.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Utilities.Randomization;
using PiQApi.Core.Resilience.Interfaces;
using PiQApi.Core.Utilities.RandomProviders;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Implementation of the policy factory
/// </summary>
public class PiQPolicyFactory : IPiQPolicyFactory
{
    private readonly ILogger<PiQPolicyFactory> _logger;
    private readonly PiQTimeoutOptions _timeoutOptions;
    private readonly PiQRetryOptions _retryOptions;
    private readonly PiQCircuitBreakerOptions _circuitBreakerOptions;
    private readonly PiQBulkheadOptions _bulkheadOptions;
    private readonly IPiQRandomProvider _randomProvider;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, int, TimeSpan, Polly.Context, Exception?> LogRetryMessage =
        LoggerMessage.Define<string, int, TimeSpan, Polly.Context>(
            LogLevel.Warning,
            new EventId(1, "PolicyRetry"),
            "Retry {ErrorType} - Attempt {RetryCount} after {RetryDelay}. Context: {@Context}");

    private static readonly Action<ILogger, TimeSpan, Polly.Context, Exception?> LogCircuitBrokenMessage =
        LoggerMessage.Define<TimeSpan, Polly.Context>(
            LogLevel.Warning,
            new EventId(2, "PolicyCircuitBroken"),
            "Circuit broken for {BreakDuration}. Context: {@Context}");

    private static readonly Action<ILogger, Polly.Context, Exception?> LogCircuitResetMessage =
        LoggerMessage.Define<Polly.Context>(
            LogLevel.Information,
            new EventId(3, "PolicyCircuitReset"),
            "Circuit reset. Context: {@Context}");

    private static readonly Action<ILogger, ResiliencePolicyType, Exception?> LogUnknownPolicyType =
        LoggerMessage.Define<ResiliencePolicyType>(
            LogLevel.Warning,
            new EventId(4, "UnknownPolicyType"),
            "Unknown policy type: {PolicyType}, using default policy");

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQPolicyFactory"/> class
    /// </summary>
    public PiQPolicyFactory(
        ILogger<PiQPolicyFactory> logger,
        PiQTimeoutOptions timeoutOptions,
        PiQRetryOptions retryOptions,
        PiQCircuitBreakerOptions circuitBreakerOptions,
        PiQBulkheadOptions bulkheadOptions,
        IPiQRandomProvider? randomProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeoutOptions = timeoutOptions ?? throw new ArgumentNullException(nameof(timeoutOptions));
        _retryOptions = retryOptions ?? throw new ArgumentNullException(nameof(retryOptions));
        _circuitBreakerOptions = circuitBreakerOptions ?? throw new ArgumentNullException(nameof(circuitBreakerOptions));
        _bulkheadOptions = bulkheadOptions ?? throw new ArgumentNullException(nameof(bulkheadOptions));
        _randomProvider = randomProvider ?? new PiQSystemRandomProvider();
    }

    /// <summary>
    /// Creates a policy for the specified policy type
    /// </summary>
    public IAsyncPolicy<T> CreatePolicy<T>(ResiliencePolicyType policyType)
    {
        // Use safer pattern matching approach instead of direct enum comparison
        // This way if the enum values don't exactly match, we can still handle it
        string policyTypeName = policyType.ToString();

        if (policyType == ResiliencePolicyType.Default)
            return CreateDefaultPolicy<T>();

        if (policyType == ResiliencePolicyType.Authentication)
            return CreateAuthenticationPolicy<T>();

        if (policyTypeName.Contains("Timeout", StringComparison.Ordinal))
            return CreateTimeoutPolicy<T>(_timeoutOptions.DefaultTimeout);

        if (policyTypeName.Contains("Circuit", StringComparison.Ordinal))
            return CreateCircuitBreakerPolicy<T>();

        if (policyTypeName.Contains("Bulk", StringComparison.Ordinal))
            return CreateBulkheadPolicy<T>();

        // Default fallback
        LogUnknownPolicyType(_logger, policyType, null);
        return CreateDefaultPolicy<T>();
    }

    /// <summary>
    /// Creates a composite policy from multiple policy types
    /// </summary>
    /// <typeparam name="T">Type of result</typeparam>
    /// <param name="policyTypes">Collection of policy types to combine</param>
    /// <returns>Composite policy</returns>
    public IAsyncPolicy<T> CreateCompositePolicy<T>(IEnumerable<ResiliencePolicyType> policyTypes)
    {
        ArgumentNullException.ThrowIfNull(policyTypes);

        IAsyncPolicy<T>? policy = null;

        foreach (var policyType in policyTypes)
        {
            var currentPolicy = CreatePolicy<T>(policyType);
            policy = policy == null ? currentPolicy : policy.WrapAsync(currentPolicy);
        }

        return policy ?? Policy.NoOpAsync<T>();
    }

    /// <summary>
    /// Creates a default policy
    /// </summary>
    public IAsyncPolicy<T> CreateDefaultPolicy<T>()
    {
        return Policy<T>
            .Handle<Exception>(IsTransientException)
            .WaitAndRetryAsync(
                _retryOptions.MaxRetryAttempts,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) +
                          TimeSpan.FromMilliseconds(_retryOptions.UseJitter ? GetRandomDelay(0, 100) : 0),
                (outcome, timeSpan, retryCount, context) =>
                {
                    // Extract the exception if present, otherwise log the context 
                    var exception = outcome.Exception;
                    var errorType = exception?.GetType().Name ?? "Unknown";
                    LogRetryMessage(_logger, errorType, retryCount, timeSpan, context, exception);
                });
    }

    /// <summary>
    /// Creates an authentication policy
    /// </summary>
    public IAsyncPolicy<T> CreateAuthenticationPolicy<T>()
    {
        return Policy<T>
            .Handle<Exception>(IsAuthenticationException)
            .WaitAndRetryAsync(
                _retryOptions.MaxAuthRetryAttempts,
                attempt => TimeSpan.FromSeconds(Math.Pow(1.5, attempt)) +
                          TimeSpan.FromMilliseconds(_retryOptions.UseJitter ? GetRandomDelay(0, 100) : 0),
                (outcome, timeSpan, retryCount, context) =>
                {
                    var exception = outcome.Exception;
                    var errorType = exception?.GetType().Name ?? "Authentication";
                    LogRetryMessage(_logger, errorType, retryCount, timeSpan, context, exception);
                });
    }

    /// <summary>
    /// Creates a timeout policy
    /// </summary>
    public IAsyncPolicy<T> CreateTimeoutPolicy<T>(TimeSpan timeout)
    {
        return Policy.TimeoutAsync<T>(timeout, TimeoutStrategy.Pessimistic);
    }

    /// <summary>
    /// Creates a bulkhead policy
    /// </summary>
    public IAsyncPolicy<T> CreateBulkheadPolicy<T>()
    {
        return Policy.BulkheadAsync<T>(
            _bulkheadOptions.MaxConcurrentOperations,
            _bulkheadOptions.MaxQueueSize);
    }

    /// <summary>
    /// Creates a circuit breaker policy
    /// </summary>
    public IAsyncPolicy<T> CreateCircuitBreakerPolicy<T>()
    {
        // Calculate exceptions allowed before breaking from failure threshold and minimum throughput
        int exceptionsAllowed = (int)(_circuitBreakerOptions.FailureThreshold * _circuitBreakerOptions.MinimumThroughput);
        exceptionsAllowed = Math.Max(1, exceptionsAllowed); // Ensure at least 1

        return Policy<T>
            .Handle<Exception>(IsTransientException)
            .CircuitBreakerAsync(
                exceptionsAllowed,
                _circuitBreakerOptions.DurationOfBreak,
                (outcome, breakDuration, context) =>
                {
                    var exception = outcome.Exception;
                    LogCircuitBrokenMessage(_logger, breakDuration, context, exception);
                },
                context =>
                {
                    LogCircuitResetMessage(_logger, context, null);
                });
    }

    /// <summary>
    /// Determines whether an exception is authentication-related
    /// </summary>
    private bool IsAuthenticationException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        // Check for specific authentication exception types
        if (ex.GetType().Name.Contains("Authentication", StringComparison.Ordinal) ||
            ex.GetType().Name.Contains("Auth", StringComparison.Ordinal) ||
            ex.GetType().Name.Contains("Credential", StringComparison.Ordinal) ||
            ex.GetType().Name.Contains("Token", StringComparison.Ordinal))
        {
            return true;
        }

        // Check for authentication-related error messages
        string message = ex.Message.ToUpperInvariant();
        return message.Contains("AUTHENTICATION", StringComparison.Ordinal) ||
               message.Contains("AUTH", StringComparison.Ordinal) ||
               message.Contains("TOKEN", StringComparison.Ordinal) ||
               message.Contains("CREDENTIAL", StringComparison.Ordinal) ||
               message.Contains("UNAUTHORIZED", StringComparison.Ordinal) ||
               message.Contains("PERMISSION", StringComparison.Ordinal) ||
               message.Contains("LOGIN", StringComparison.Ordinal) ||
               message.Contains("IDENTITY", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether an exception is transient
    /// </summary>
    public bool IsTransientException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        // Specific exception types known to be transient
        if ((ex is TimeoutException) ||
            (ex is System.Net.Sockets.SocketException) ||
            (ex is HttpRequestException httpEx &&
             (httpEx.StatusCode >= System.Net.HttpStatusCode.InternalServerError ||
              httpEx.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
              httpEx.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
              httpEx.StatusCode == System.Net.HttpStatusCode.GatewayTimeout ||
              httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests)) ||
            (ex is IOException ioEx && IsTransientIOException(ioEx)) ||
            ex.GetType().Name.Contains("CommunicationException", StringComparison.Ordinal))
        {
            return true;
        }

        // Check for cancellation due to timeout
        if ((ex is OperationCanceledException) && ex.Message.Contains("timeout", StringComparison.Ordinal))
        {
            return true;
        }

        // Check inner exception for transient causes
        if ((ex.InnerException != null) && IsTransientException(ex.InnerException))
        {
            return true;
        }

        // Message-based detection as fallback
        string message = ex.Message.ToUpperInvariant();
        return
            // Timeout related
            message.Contains("TIMEOUT", StringComparison.Ordinal) ||

            // Connection and network related
            message.Contains("CONNECTION", StringComparison.Ordinal) ||
            message.Contains("NETWORK", StringComparison.Ordinal) ||
            message.Contains("UNREACHABLE", StringComparison.Ordinal) ||
            message.Contains("REFUSED", StringComparison.Ordinal) ||
            message.Contains("RESET", StringComparison.Ordinal) ||

            // Service availability related
            message.Contains("TEMPORARILY UNAVAILABLE", StringComparison.Ordinal) ||
            message.Contains("SERVER BUSY", StringComparison.Ordinal) ||
            message.Contains("TOO MANY REQUESTS", StringComparison.Ordinal) ||
            message.Contains("RATE LIMIT", StringComparison.Ordinal) ||
            message.Contains("THROTTLE", StringComparison.Ordinal) ||
            message.Contains("OVERLOADED", StringComparison.Ordinal) ||
            message.Contains("TRY AGAIN", StringComparison.Ordinal)
        ;
    }

    /// <summary>
    /// Determines whether an IO exception is transient
    /// </summary>
    private static bool IsTransientIOException(IOException ex)
    {
        // Additional checks specific to IOException types
        string message = ex.Message.ToUpperInvariant();
        return message.Contains("CONNECTION", StringComparison.Ordinal) ||
               message.Contains("NETWORK", StringComparison.Ordinal) ||
               message.Contains("PIPE", StringComparison.Ordinal) ||
               message.Contains("RESET", StringComparison.Ordinal) ||
               message.Contains("CLOSED", StringComparison.Ordinal) ||
               message.Contains("BROKEN", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets a random delay between min and max values
    /// </summary>
    private int GetRandomDelay(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
            return minValue;

        // Use the injected random provider for deterministic testing
        return _randomProvider.NextIntInRange(minValue, maxValue);
    }
}
