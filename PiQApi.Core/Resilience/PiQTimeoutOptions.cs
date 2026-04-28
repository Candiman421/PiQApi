// PiQApi.Core/Resilience/PiQTimeoutOptions.cs
using System.ComponentModel.DataAnnotations;

namespace PiQApi.Core.Resilience;

/// <summary>
/// Configuration options for timeout policy
/// </summary>
public record PiQTimeoutOptions
{
    /// <summary>
    /// Gets or sets the default timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int DefaultTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets or sets the search timeout in seconds
    /// </summary>
    [Range(1, 600)]
    public int SearchTimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Gets or sets the authentication timeout in seconds
    /// </summary>
    [Range(1, 60)]
    public int AuthTimeoutSeconds { get; init; } = 15;

    /// <summary>
    /// Gets the default timeout as a TimeSpan
    /// </summary>
    public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(DefaultTimeoutSeconds);

    /// <summary>
    /// Gets the search timeout as a TimeSpan
    /// </summary>
    public TimeSpan SearchTimeout => TimeSpan.FromSeconds(SearchTimeoutSeconds);

    /// <summary>
    /// Gets the authentication timeout as a TimeSpan
    /// </summary>
    public TimeSpan AuthTimeout => TimeSpan.FromSeconds(AuthTimeoutSeconds);
}