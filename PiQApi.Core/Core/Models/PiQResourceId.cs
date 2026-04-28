// PiQApi.Core/Core/Models/PiQResourceId.cs
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Utilities.Time;

namespace PiQApi.Core.Core.Models;

/// <summary>
/// Represents a unique identifier for a resource
/// </summary>
public sealed class PiQResourceId : IEquatable<PiQResourceId?>
{
    // Replaced direct implementation dependency with factory access
    private static IPiQTimeProvider DefaultTimeProvider => PiQTimeProviderFactory.Current;

    /// <summary>
    /// Gets the resource identifier value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the resource type
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTime CreatedUtc { get; }

    /// <summary>
    /// Creates a new resource identifier
    /// </summary>
    /// <param name="type">Resource type</param>
    /// <param name="value">Resource identifier value</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public PiQResourceId(string type, string value, IPiQTimeProvider? timeProvider = null)
        : this(type, value, (timeProvider ?? DefaultTimeProvider).UtcNow)
    {
    }

    /// <summary>
    /// Creates a new resource identifier with specified creation time
    /// </summary>
    /// <param name="type">Resource type</param>
    /// <param name="value">Resource identifier value</param>
    /// <param name="createdUtc">Creation timestamp</param>
    public PiQResourceId(string type, string value, DateTime createdUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentException.ThrowIfNullOrEmpty(value);

        Type = type;
        Value = value;
        CreatedUtc = createdUtc;
    }

    /// <summary>
    /// Creates a new resource identifier with a different type
    /// </summary>
    /// <param name="type">New resource type</param>
    /// <returns>A new resource identifier with the new type</returns>
    public PiQResourceId WithType(string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);
        return new PiQResourceId(type, Value, CreatedUtc);
    }

    /// <summary>
    /// Creates a new resource identifier with a different value
    /// </summary>
    /// <param name="value">New resource value</param>
    /// <returns>A new resource identifier with the new value</returns>
    public PiQResourceId WithValue(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        return new PiQResourceId(Type, value, CreatedUtc);
    }

    /// <summary>
    /// Tries to parse a string into a resource identifier
    /// </summary>
    /// <param name="input">Input string in format "type:value"</param>
    /// <param name="resourceId">Output resource identifier</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool TryParse(string? input, out PiQResourceId? resourceId, IPiQTimeProvider? timeProvider = null)
    {
        resourceId = null;

        if (string.IsNullOrEmpty(input))
            return false;

        // Check for the first colon
        int colonIndex = input.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex <= 0 || colonIndex == input.Length - 1)
            return false;

        string type = input[..colonIndex];
        string value = input[(colonIndex + 1)..];

        if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
            return false;

        try
        {
            resourceId = new PiQResourceId(type, value, timeProvider);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of the resource ID
    /// </summary>
    public override string ToString()
    {
        return $"{Type}:{Value}";
    }

    /// <summary>
    /// Determines if this resource ID is equal to another
    /// </summary>
    /// <param name="other">Other resource ID</param>
    /// <returns>True if the resource IDs are equal</returns>
    public bool Equals(PiQResourceId? other)
    {
        return other is not null
                     && (ReferenceEquals(this, other)
                     || (string.Equals(Value, other.Value, StringComparison.Ordinal) &&
                        string.Equals(Type, other.Type, StringComparison.Ordinal)));
    }

    /// <summary>
    /// Determines if this resource ID is equal to another object
    /// </summary>
    /// <param name="obj">Other object</param>
    /// <returns>True if the objects are equal</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is PiQResourceId other && Equals(other));
    }

    /// <summary>
    /// Gets a hash code for the resource ID
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Type);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(PiQResourceId? left, PiQResourceId? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(PiQResourceId? left, PiQResourceId? right)
    {
        return !Equals(left, right);
    }
}
