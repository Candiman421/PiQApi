// PiQApi.Core/Utilities/RandomProviders/PiQRandomProviderFactory.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Utilities.Randomization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PiQApi.Core.Utilities.RandomProviders;

/// <summary>
/// Factory for creating random providers with different security characteristics
/// </summary>
public class PiQRandomProviderFactory
{
    // Initialize static fields directly to avoid analyzer warnings
    private static IPiQRandomProvider _fastProvider = PiQSystemRandomProvider.CreateFast();
    private static IPiQRandomProvider _secureProvider = PiQSystemRandomProvider.CreateSecure();
    private static readonly PiQRandomProviderFactory _instance = new PiQRandomProviderFactory();

    // Lock object for thread-safe initialization
    private static readonly object _lock = new object();

    private readonly ILoggerFactory _loggerFactory;
    private readonly IPiQCorrelationContext? _correlationContext;

    /// <summary>
    /// Gets the singleton instance of the factory
    /// </summary>
    public static PiQRandomProviderFactory Instance => _instance;

    /// <summary>
    /// Gets the default fast random provider (non-cryptographic)
    /// </summary>
    public static IPiQRandomProvider FastProvider => _fastProvider;

    /// <summary>
    /// Gets the default secure random provider (cryptographic)
    /// </summary>
    public static IPiQRandomProvider SecureProvider => _secureProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQRandomProviderFactory"/> class
    /// </summary>
    public PiQRandomProviderFactory()
        : this(NullLoggerFactory.Instance, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQRandomProviderFactory"/> class
    /// </summary>
    /// <param name="loggerFactory">Logger factory for creating loggers</param>
    /// <param name="correlationContext">Correlation context for tracing operations</param>
    public PiQRandomProviderFactory(
        ILoggerFactory loggerFactory,
        IPiQCorrelationContext? correlationContext)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _correlationContext = correlationContext;
    }

    /// <summary>
    /// Creates a fast random provider suitable for non-security-sensitive operations
    /// </summary>
    /// <returns>A fast, non-cryptographic random provider</returns>
    public IPiQRandomProvider CreateFastProvider()
    {
        // Track operation in correlation context if available
        _correlationContext?.AddProperty("Operation", "CreateFastProvider");

        var logger = _loggerFactory.CreateLogger<PiQSystemRandomProvider>();
        return PiQSystemRandomProvider.CreateFast(logger, _correlationContext);
    }

    /// <summary>
    /// Creates a secure random provider suitable for security-sensitive operations
    /// </summary>
    /// <returns>A cryptographically secure random provider</returns>
    public IPiQRandomProvider CreateSecureProvider()
    {
        // Track operation in correlation context if available
        _correlationContext?.AddProperty("Operation", "CreateSecureProvider");

        var logger = _loggerFactory.CreateLogger<PiQSystemRandomProvider>();
        return PiQSystemRandomProvider.CreateSecure(logger, _correlationContext);
    }

    /// <summary>
    /// Creates a random provider with the specified security level
    /// </summary>
    /// <param name="useCryptographic">Whether to use cryptographic random generation</param>
    /// <returns>A random provider configured with the requested security level</returns>
    public IPiQRandomProvider CreateProvider(bool useCryptographic)
    {
        // Track operation in correlation context if available
        _correlationContext?.AddProperty("Operation", "CreateProvider");
        _correlationContext?.AddProperty("UseCryptographic", useCryptographic);

        var logger = _loggerFactory.CreateLogger<PiQSystemRandomProvider>();
        return PiQSystemRandomProvider.Create(useCryptographic, logger, _correlationContext);
    }

    /// <summary>
    /// Configures the factory with specific providers
    /// </summary>
    /// <param name="fastProvider">Provider for non-security-sensitive operations</param>
    /// <param name="secureProvider">Provider for security-sensitive operations</param>
    public static void Configure(IPiQRandomProvider fastProvider, IPiQRandomProvider secureProvider)
    {
        ArgumentNullException.ThrowIfNull(fastProvider);
        ArgumentNullException.ThrowIfNull(secureProvider);

        lock (_lock)
        {
            _fastProvider = fastProvider;
            _secureProvider = secureProvider;
        }
    }

    /// <summary>
    /// Resets providers to default implementations
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _fastProvider = PiQSystemRandomProvider.CreateFast();
            _secureProvider = PiQSystemRandomProvider.CreateSecure();
        }
    }
}