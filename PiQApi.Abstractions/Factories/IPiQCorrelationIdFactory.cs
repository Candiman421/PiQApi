// PiQApi.Abstractions/Factories/IPiQCorrelationIdFactory.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Abstractions.Factories;

/// <summary>
/// Factory for creating correlation identifiers
/// </summary>
public interface IPiQCorrelationIdFactory
{
    /// <summary>
    /// Creates a new correlation ID with randomly generated identifier
    /// </summary>
    /// <returns>A new correlation ID</returns>
    IPiQCorrelationId Create();

    /// <summary>
    /// Creates a correlation ID from an existing identifier
    /// </summary>
    /// <param name="existingId">The existing identifier to use</param>
    /// <returns>A correlation ID with the specified identifier</returns>
    IPiQCorrelationId CreateFromExisting(string existingId);
}
