// CertApi.Abstractions/Factories/ICertCorrelationIdFactory.cs
using CertApi.Abstractions.Core;

namespace CertApi.Abstractions.Factories;

/// <summary>
/// Factory for creating correlation identifiers
/// </summary>
public interface ICertCorrelationIdFactory
{
    /// <summary>
    /// Creates a new correlation ID with randomly generated identifier
    /// </summary>
    /// <returns>A new correlation ID</returns>
    ICertCorrelationId Create();

    /// <summary>
    /// Creates a correlation ID from an existing identifier
    /// </summary>
    /// <param name="existingId">The existing identifier to use</param>
    /// <returns>A correlation ID with the specified identifier</returns>
    ICertCorrelationId CreateFromExisting(string existingId);
}
