// PiQApi.Core/Utilities/RandomProviders/PiQSystemRandomProvider.cs
using System.Security.Cryptography;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Utilities.Randomization;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Utilities.RandomProviders;

/// <summary>
/// Implementation of IPiQRandomProvider that uses appropriate random generation based on context
/// </summary>
public class PiQSystemRandomProvider : IPiQRandomProvider
{
    // For performance-sensitive non-security operations like jitter
    private static readonly System.Random _fastRandom = System.Random.Shared;
    private readonly ILogger<PiQSystemRandomProvider>? _logger;
    private readonly IPiQCorrelationContext? _correlationContext;

    /// <summary>
    /// Flag indicating whether to use cryptographically strong random generation
    /// Defaults to false for performance in retry scenarios
    /// </summary>
    private readonly bool _useCryptographicRandom;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, string, Exception?> LogRandomOperation =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, "RandomGeneration"),
            "[{CorrelationId}] Generated random value using {GenerationType}");

    /// <summary>
    /// Initializes a new instance with default settings for retry/jitter scenarios
    /// </summary>
    public PiQSystemRandomProvider() : this(false, null, null) { }

    /// <summary>
    /// Initializes a new instance with specified security settings
    /// </summary>
    /// <param name="useCryptographicRandom">When true, uses cryptographically strong random generation</param>
    public PiQSystemRandomProvider(bool useCryptographicRandom)
        : this(useCryptographicRandom, null, null) { }

    /// <summary>
    /// Initializes a new instance with specified security settings, logger, and correlation context
    /// </summary>
    /// <param name="useCryptographicRandom">When true, uses cryptographically strong random generation</param>
    /// <param name="logger">Logger for tracking random generation</param>
    /// <param name="correlationContext">Correlation context for tracing</param>
    public PiQSystemRandomProvider(
        bool useCryptographicRandom,
        ILogger<PiQSystemRandomProvider>? logger,
        IPiQCorrelationContext? correlationContext)
    {
        _useCryptographicRandom = useCryptographicRandom;
        _logger = logger;
        _correlationContext = correlationContext;

        // Log initialization if context is available
        if (_logger != null && _correlationContext != null)
        {
            var generationType = _useCryptographicRandom ? "Cryptographic" : "Fast";
            LogRandomOperation(_logger, _correlationContext.CorrelationId, generationType, null);
        }
    }

    /// <summary>
    /// Gets whether this provider uses cryptographically strong random number generation
    /// </summary>
    public bool UsesCryptographicRandom => _useCryptographicRandom;

    /// <summary>
    /// Returns a non-negative random integer
    /// </summary>
    public int NextInt()
    {
        // Track operation in correlation context if available
        _correlationContext?.AddProperty("RandomOperation", "NextInt");

        int result;
        if (_useCryptographicRandom)
        {
            // Use secure random for security-sensitive operations
            result = RandomNumberGenerator.GetInt32(0, int.MaxValue);
            LogRandomUsage("Cryptographic");
        }
        else
        {
            // Use fast random for performance-sensitive operations
            // This is appropriate for jitter, retry delays, and other non-security operations
            // Suppressed security warning since this is intentional
#pragma warning disable CA5394 // This is intentionally using non-cryptographic random for performance
            result = _fastRandom.Next();
#pragma warning restore CA5394
            LogRandomUsage("Fast");
        }

        return result;
    }

    /// <summary>
    /// Returns a non-negative random integer less than the specified maximum
    /// </summary>
    public int NextIntUnderMax(int maxValue)
    {
        if (maxValue < 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "Maximum value must be non-negative");

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("RandomOperation", "NextIntUnderMax");
        _correlationContext?.AddProperty("MaxValue", maxValue);

        int result;
        if (_useCryptographicRandom)
        {
            result = RandomNumberGenerator.GetInt32(maxValue);
            LogRandomUsage("Cryptographic");
        }
        else
        {
            // Suppressed security warning since this is intentional
#pragma warning disable CA5394 // This is intentionally using non-cryptographic random for performance
            result = _fastRandom.Next(maxValue);
#pragma warning restore CA5394
            LogRandomUsage("Fast");
        }

        return result;
    }

    /// <summary>
    /// Returns a random integer within the specified range
    /// </summary>
    public int NextIntInRange(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
            throw new ArgumentOutOfRangeException(nameof(minValue), "Minimum value must be less than maximum value");

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("RandomOperation", "NextIntInRange");
        _correlationContext?.AddProperty("MinValue", minValue);
        _correlationContext?.AddProperty("MaxValue", maxValue);

        int result;
        if (_useCryptographicRandom)
        {
            result = RandomNumberGenerator.GetInt32(minValue, maxValue);
            LogRandomUsage("Cryptographic");
        }
        else
        {
            // Suppressed security warning since this is intentional
#pragma warning disable CA5394 // This is intentionally using non-cryptographic random for performance
            result = _fastRandom.Next(minValue, maxValue);
#pragma warning restore CA5394
            LogRandomUsage("Fast");
        }

        return result;
    }

    /// <summary>
    /// Fills the elements of a specified array of bytes with random numbers
    /// </summary>
    public void NextBytes(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        // Track operation in correlation context if available
        _correlationContext?.AddProperty("RandomOperation", "NextBytes");
        _correlationContext?.AddProperty("BufferLength", buffer.Length);

        if (_useCryptographicRandom)
        {
            RandomNumberGenerator.Fill(buffer);
            LogRandomUsage("Cryptographic");
        }
        else
        {
            // Suppressed security warning since this is intentional
#pragma warning disable CA5394 // This is intentionally using non-cryptographic random for performance
            _fastRandom.NextBytes(buffer);
#pragma warning restore CA5394
            LogRandomUsage("Fast");
        }
    }

    /// <summary>
    /// Returns a random floating-point number between 0.0 and 1.0
    /// </summary>
    public double NextDouble()
    {
        // Track operation in correlation context if available
        _correlationContext?.AddProperty("RandomOperation", "NextDouble");

        double result;
        if (_useCryptographicRandom)
        {
            result = GetCryptographicDouble();
            LogRandomUsage("Cryptographic");
        }
        else
        {
            // Suppressed security warning since this is intentional
#pragma warning disable CA5394 // This is intentionally using non-cryptographic random for performance
            result = _fastRandom.NextDouble();
#pragma warning restore CA5394
            LogRandomUsage("Fast");
        }

        return result;
    }

    /// <summary>
    /// Creates a factory method to get a provider with the specified security level
    /// </summary>
    /// <param name="useCryptographic">Whether to use cryptographic random generation</param>
    /// <param name="logger">Optional logger for tracking</param>
    /// <param name="correlationContext">Optional correlation context for tracing</param>
    /// <returns>A random provider configured with the requested security level</returns>
    public static IPiQRandomProvider Create(
        bool useCryptographic,
        ILogger<PiQSystemRandomProvider>? logger = null,
        IPiQCorrelationContext? correlationContext = null)
    {
        return new PiQSystemRandomProvider(useCryptographic, logger, correlationContext);
    }

    /// <summary>
    /// Creates a cryptographically secure random provider
    /// </summary>
    /// <param name="logger">Optional logger for tracking</param>
    /// <param name="correlationContext">Optional correlation context for tracing</param>
    /// <returns>A cryptographically secure random provider</returns>
    public static IPiQRandomProvider CreateSecure(
        ILogger<PiQSystemRandomProvider>? logger = null,
        IPiQCorrelationContext? correlationContext = null)
    {
        return new PiQSystemRandomProvider(true, logger, correlationContext);
    }

    /// <summary>
    /// Creates a fast, non-cryptographic random provider for performance-sensitive scenarios
    /// </summary>
    /// <param name="logger">Optional logger for tracking</param>
    /// <param name="correlationContext">Optional correlation context for tracing</param>
    /// <returns>A fast, non-cryptographic random provider</returns>
    public static IPiQRandomProvider CreateFast(
        ILogger<PiQSystemRandomProvider>? logger = null,
        IPiQCorrelationContext? correlationContext = null)
    {
        return new PiQSystemRandomProvider(false, logger, correlationContext);
    }

    private static double GetCryptographicDouble()
    {
        var bytes = new byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(bytes);
        var randomValue = BitConverter.ToUInt64(bytes, 0);
        return randomValue / (double)ulong.MaxValue;
    }

    private void LogRandomUsage(string generationType)
    {
        if (_logger != null && _correlationContext != null)
        {
            LogRandomOperation(_logger, _correlationContext.CorrelationId, generationType, null);
        }
    }
}