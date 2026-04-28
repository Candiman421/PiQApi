// PiQApi.Abstractions/Factories/ICorrelationIdFactory.cs
using PiQApi.Abstractions.Core.Models;

namespace PiQApi.Abstractions.Factories
{
    /// <summary>
    /// Factory for creating correlation identifiers
    /// </summary>
    public interface ICorrelationIdFactory
    {
        /// <summary>
        /// Creates a new correlation identifier
        /// </summary>
        /// <returns>A new correlation identifier</returns>
        CorrelationId Create();

        /// <summary>
        /// Creates a new correlation identifier from an existing ID
        /// </summary>
        /// <param name="existingId">Existing identifier</param>
        /// <returns>A new correlation identifier</returns>
        CorrelationId CreateFromExisting(string existingId);
    }
}