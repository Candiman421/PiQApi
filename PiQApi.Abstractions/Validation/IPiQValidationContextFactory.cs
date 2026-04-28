// PiQApi.Abstractions/Validation/IPiQValidationContextFactory.cs
using PiQApi.Abstractions.Validation;

namespace PiQApi.Abstractions.Validation;

/// <summary>
/// Interface for creating validation contexts
/// </summary>
public interface IPiQValidationContextFactory
{
    /// <summary>
    /// Creates a validation context for a service
    /// </summary>
    /// <param name="correlationId">The correlation ID for tracing</param>
    /// <param name="serviceVersion">The service version to use</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A configured validation context</returns>
    IPiQValidationContext CreateForService(
        string correlationId,
        string serviceVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a validation context with a generated correlation ID
    /// </summary>
    /// <param name="serviceVersion">The service version to use</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A configured validation context</returns>
    IPiQValidationContext CreateForService(
        string serviceVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds service version to an existing context
    /// </summary>
    /// <param name="context">The existing validation context</param>
    /// <param name="serviceVersion">The service version to add</param>
    /// <returns>The updated validation context</returns>
    IPiQValidationContext AddServiceVersion(
        IPiQValidationContext context,
        string serviceVersion);
}