// PiQApi.Abstractions/Utilities/Time/ICertTimeProvider.cs
namespace PiQApi.Abstractions.Utilities.Time;

/// <summary>
/// Provides an abstraction for time-related operations to improve testability
/// </summary>
public interface ICertTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets a timestamp representing the current time
    /// </summary>
    TimeSpan GetTimestamp();

    /// <summary>
    /// Gets the elapsed time between the current time and a specified timestamp
    /// </summary>
    /// <param name="timestamp">The starting timestamp</param>
    /// <returns>A TimeSpan representing the elapsed interval</returns>
    TimeSpan GetElapsedTime(TimeSpan timestamp);
}
