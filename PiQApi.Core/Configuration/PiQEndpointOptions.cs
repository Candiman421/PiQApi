// PiQApi.Core/Configuration/PiQEndpointOptions.cs
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Configuration;

/// <summary>
/// Options for endpoint configuration
/// </summary>
public record PiQEndpointOptions
{
    /// <summary>
    /// Gets or sets the endpoint name
    /// </summary>
    [Required]
    public string EndpointName { get; init; }

    /// <summary>
    /// Gets or sets the endpoint URI
    /// </summary>
    public Uri? EndpointUri { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of retries
    /// </summary>
    [Range(0, 10)]
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets or sets the retry delay in seconds
    /// </summary>
    [Range(0, 60)]
    public int RetryDelaySeconds { get; init; } = 1;

    /// <summary>
    /// Gets or sets the timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the additional settings
    /// </summary>
    public IReadOnlyDictionary<string, string> AdditionalSettings { get; init; } =
        ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="PiQEndpointOptions"/>
    /// </summary>
    /// <param name="endpointName">The endpoint name</param>
    public PiQEndpointOptions(string endpointName)
    {
        EndpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
    }

    /// <summary>
    /// Creates a new options instance with a different endpoint URI
    /// </summary>
    public PiQEndpointOptions WithEndpointUri(Uri endpointUri) =>
        this with { EndpointUri = endpointUri ?? throw new ArgumentNullException(nameof(endpointUri)) };

    /// <summary>
    /// Creates a new options instance with different retry settings
    /// </summary>
    public PiQEndpointOptions WithRetrySettings(int maxRetries, int retryDelaySeconds)
    {
        if (maxRetries < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), maxRetries, "Max retries must be non-negative");
        if (retryDelaySeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(retryDelaySeconds), retryDelaySeconds, "Retry delay must be non-negative");

        return this with
        {
            MaxRetries = maxRetries,
            RetryDelaySeconds = retryDelaySeconds
        };
    }

    /// <summary>
    /// Creates a new options instance with a different timeout
    /// </summary>
    public PiQEndpointOptions WithTimeout(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), timeoutSeconds, "Timeout must be positive");

        return this with { TimeoutSeconds = timeoutSeconds };
    }

    /// <summary>
    /// Creates a new options instance with an additional setting
    /// </summary>
    public PiQEndpointOptions WithSetting(string key, string value)
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