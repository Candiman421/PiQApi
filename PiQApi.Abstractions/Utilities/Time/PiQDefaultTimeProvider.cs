// PiQApi.Abstractions/Utilities/Time/PiQDefaultTimeProvider.cs
using System.Diagnostics;

namespace PiQApi.Abstractions.Utilities.Time;

/// <summary>
/// Default implementation of IPiQTimeProvider for use within Abstractions project.
/// This avoids circular dependencies with the Core project.
/// </summary>
public class PiQDefaultTimeProvider : IPiQTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Gets a timestamp representing the current time
    /// </summary>
    public TimeSpan GetTimestamp()
    {
        // Convert long to TimeSpan to match the interface
        return TimeSpan.FromTicks(Stopwatch.GetTimestamp());
    }

    /// <summary>
    /// Gets the elapsed time between the current time and a specified timestamp
    /// </summary>
    /// <param name="timestamp">The starting timestamp</param>
    /// <returns>A TimeSpan representing the elapsed interval</returns>
    public TimeSpan GetElapsedTime(TimeSpan timestamp)
    {
        // Convert TimeSpan to long for Stopwatch.GetElapsedTime, then it returns TimeSpan
        return Stopwatch.GetElapsedTime(timestamp.Ticks);
    }
}
