// PiQApi.Core/Utilities/Time/PiQTimeProviderFactory.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Utilities.Time;

/// <summary>
/// Factory for accessing time provider instances
/// </summary>
public class PiQTimeProviderFactory : IPiQTimeProviderFactory
{
    // Use volatile for thread safety without locking in reads
    private static volatile IPiQTimeProvider _current = new PiQSystemTimeProvider();
    private static readonly PiQTimeProviderFactory _instance = new PiQTimeProviderFactory();

    // Lock object for thread-safe initialization
    private static readonly object _lock = new object();

    private readonly ILogger<PiQTimeProviderFactory>? _logger;
    private readonly IPiQCorrelationContext? _correlationContext;

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
    public static PiQTimeProviderFactory Instance => _instance;

    /// <summary>
    /// Gets the current IPiQTimeProvider instance
    /// </summary>
    public static IPiQTimeProvider Current => _current;

    /// <summary>
    /// Gets the current IPiQTimeProvider instance (interface implementation)
    /// </summary>
    IPiQTimeProvider IPiQTimeProviderFactory.Current => PiQTimeProviderFactory.Current;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQTimeProviderFactory"/> class
    /// </summary>
    public PiQTimeProviderFactory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQTimeProviderFactory"/> class with logging and correlation
    /// </summary>
    /// <param name="logger">Logger for operations</param>
    /// <param name="correlationContext">Correlation context for tracing</param>
    public PiQTimeProviderFactory(
        ILogger<PiQTimeProviderFactory> logger,
        IPiQCorrelationContext? correlationContext = null)
    {
        _logger = logger;
        _correlationContext = correlationContext;
    }

    /// <summary>
    /// Configures the factory with a specific IPiQTimeProvider implementation (static)
    /// </summary>
    /// <param name="provider">The time provider to use</param>
    public static void ConfigureStatic(IPiQTimeProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_lock)
        {
            _current = provider;
        }
    }

    /// <summary>
    /// Configures the factory with a specific IPiQTimeProvider implementation (instance method for interface)
    /// </summary>
    /// <param name="provider">The time provider to use</param>
    public void Configure(IPiQTimeProvider provider)
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
            _current = new PiQSystemTimeProvider();
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
            _current = new PiQSystemTimeProvider();
        }

        // Log the reset if logger is available
        if (_logger != null && _correlationContext != null)
        {
            LogResetProvider(_logger, _correlationContext.CorrelationId, null);
        }
    }
}