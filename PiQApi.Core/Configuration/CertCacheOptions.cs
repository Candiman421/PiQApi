// PiQApi.Core/Configuration/CertCacheOptions.cs
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Configuration;

/// <summary>
/// Configuration options for caching
/// </summary>
public record CertCacheOptions
{
    /// <summary>
    /// Gets or sets the default expiration time in minutes
    /// </summary>
    [Range(1, 1440)]
    public int DefaultExpirationMinutes { get; init; } = 60;

    /// <summary>
    /// Gets or sets the token expiration in minutes
    /// </summary>
    [Range(1, 1440)]
    public int TokenExpirationMinutes { get; init; } = 55;

    /// <summary>
    /// Gets or sets the maximum number of items in the cache
    /// </summary>
    [Range(1, 100000)]
    public int MaxItems { get; init; } = 1000;

    /// <summary>
    /// Gets or sets whether to use the memory cache
    /// </summary>
    public bool UseMemoryCache { get; init; } = true;

    /// <summary>
    /// Gets or sets the cleanup interval in minutes
    /// </summary>
    [Range(1, 1440)]
    public int CleanupIntervalMinutes { get; init; } = 15;
}