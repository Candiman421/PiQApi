// CertApi.Core/Utilities/Time/CertTimeProviderFactory.cs
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Utilities.Time;

/// <summary>
/// Factory for accessing time provider instances
/// </summary>
public class CertTimeProviderFactory : ICertTimeProviderFactory
{
    // Use volatile for thread safety without locking in reads
    private static volatile ICertTimeProvider _current = new CertSystemTimeProvider();
    private static readonly CertTimeProviderFactory _instance = new CertTimeProviderFactory();

    // Lock object for thread-safe initialization
    private static readonly object _lock = new object();

    private readonly ILogger<CertTimeProviderFactory>? _logger;
    private readonly ICertCorrelationContext? _correlationContext;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, string, Exception?> LogConfigureProvider =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, "Configure"),
            "[{CorrelationId}] Configured time provider to use {ProviderType}");

    private static readonly Action<ILogger, string, Exception?> LogResetProvider =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, "Reset"),
            "[{CorrelationId}] Reset time provider to default implementation");

    /// <summary>
    /// Gets the singleton instance of the factory
    /// </summary>
    public static CertTimeProviderFactory Instance => _instance;

    /// <summary>
    /// Gets the current ICertTimeProvider instance
    /// </summary>
    public static ICertTimeProvider Current => _current;

    /// <summary>
    /// Gets the current ICertTimeProvider instance (interface implementation)
    /// </summary>
    ICertTimeProvider ICertTimeProviderFactory.Current => CertTimeProviderFactory.Current;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertTimeProviderFactory"/> class
    /// </summary>
    public CertTimeProviderFactory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertTimeProviderFactory"/> class with logging and correlation
    /// </summary>
    /// <param name="logger">Logger for operations</param>
    /// <param name="correlationContext">Correlation context for tracing</param>
    public CertTimeProviderFactory(
        ILogger<CertTimeProviderFactory> logger,
        ICertCorrelationContext? correlationContext = null)
    {
        _logger = logger;
        _correlationContext = correlationContext;
    }

    /// <summary>
    /// Configures the factory with a specific ICertTimeProvider implementation (static)
    /// </summary>
    /// <param name="provider">The time provider to use</param>
    public static void ConfigureStatic(ICertTimeProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_lock)
        {
            _current = provider;
        }
    }

    /// <summary>
    /// Configures the factory with a specific ICertTimeProvider implementation (instance method for interface)
    /// </summary>
    /// <param name="provider">The time provider to use</param>
    public void Configure(ICertTimeProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("Operation", "ConfigureTimeProvider");
        _correlationContext?.AddProperty("ProviderType", provider.GetType().Name);

        lock (_lock)
        {
            _current = provider;
        }

        // Log the configuration if logger is available
        if (_logger != null && _correlationContext != null)
        {
            LogConfigureProvider(_logger, _correlationContext.CorrelationId, provider.GetType().Name, null);
        }
    }

    /// <summary>
    /// Resets the time provider to the default implementation (static)
    /// </summary>
    public static void ResetStatic()
    {
        lock (_lock)
        {
            _current = new CertSystemTimeProvider();
        }
    }

    /// <summary>
    /// Resets the time provider to the default implementation (instance method for interface)
    /// </summary>
    public void Reset()
    {
        // Track operation in correlation context if available
        _correlationContext?.AddProperty("Operation", "ResetTimeProvider");

        lock (_lock)
        {
            _current = new CertSystemTimeProvider();
        }

        // Log the reset if logger is available
        if (_logger != null && _correlationContext != null)
        {
            LogResetProvider(_logger, _correlationContext.CorrelationId, null);
        }
    }
}