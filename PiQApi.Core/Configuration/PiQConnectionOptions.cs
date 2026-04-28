// PiQApi.Core/Configuration/PiQConnectionOptions.cs
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Configuration;

/// <summary>
/// Options for connection configuration
/// </summary>
public record PiQConnectionOptions
{
    /// <summary>
    /// Gets or sets the endpoint configuration
    /// </summary>
    [Required]
    public PiQEndpointOptions Endpoint { get; init; } = new PiQEndpointOptions("Default");

    /// <summary>
    /// Gets or sets the connection pool size
    /// </summary>
    [Range(1, 1000)]
    public int PoolSize { get; init; } = 10;

    /// <summary>
    /// Gets or sets whether connection reuse is allowed
    /// </summary>
    public bool AllowConnectionReuse { get; init; } = true;

    /// <summary>
    /// Gets or sets the idle timeout in seconds
    /// </summary>
    [Range(1, 3600)]
    public int IdleTimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Gets or sets the connection lifetime in seconds
    /// </summary>
    [Range(1, 86400)]
    public int LifetimeSeconds { get; init; } = 300;

    /// <summary>
    /// Gets the additional settings
    /// </summary>
    public IReadOnlyDictionary<string, string> AdditionalSettings { get; init; } =
        ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Creates a new options with a different endpoint
    /// </summary>
    public PiQConnectionOptions WithEndpoint(PiQEndpointOptions endpoint) =>
        this with { Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint)) };

    /// <summary>
    /// Creates a new options with a different pool size
    /// </summary>
    public PiQConnectionOptions WithPoolSize(int poolSize)
    {
        if (poolSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(poolSize), poolSize, "Pool size must be positive");

        return this with { PoolSize = poolSize };
    }

    /// <summary>
    /// Creates a new options with different timeout settings
    /// </summary>
    public PiQConnectionOptions WithTimeoutSettings(int idleTimeoutSeconds, int lifetimeSeconds)
    {
        if (idleTimeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(idleTimeoutSeconds), idleTimeoutSeconds, "Idle timeout must be positive");
        if (lifetimeSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(lifetimeSeconds), lifetimeSeconds, "Lifetime must be positive");

        return this with
        {
            IdleTimeoutSeconds = idleTimeoutSeconds,
            LifetimeSeconds = lifetimeSeconds
        };
    }

    /// <summary>
    /// Creates a new options with a different connection reuse setting
    /// </summary>
    public PiQConnectionOptions WithConnectionReuse(bool allowConnectionReuse) =>
        this with { AllowConnectionReuse = allowConnectionReuse };

    /// <summary>
    /// Creates a new options with an additional setting
    /// </summary>
    public PiQConnectionOptions WithSetting(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);

        var newSettings = new Dictionary<string, string>(AdditionalSettings)
        {
            [key] = value
        };

        return this with { AdditionalSettings = newSettings };
    }
}