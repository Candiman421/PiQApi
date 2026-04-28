// CertApi.Ews.Core/Validation/Interfaces/IEwsValidationContextFactory.cs
using Microsoft.Exchange.WebServices.Data;
using System.Threading;

namespace CertApi.Ews.Core.Validation.Interfaces
{
    /// <summary>
    /// Factory for creating EWS-specific validation contexts
    /// </summary>
    public interface IEwsValidationContextFactory
    {
        /// <summary>
        /// Creates a validation context for EWS service operations
        /// </summary>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A configured validation context</returns>
        IEwsValidationContext CreateForEwsService(
            ExchangeVersion exchangeVersion,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a validation context for EWS service operations with correlation ID
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A configured validation context</returns>
        IEwsValidationContext CreateForEwsService(
            string correlationId,
            ExchangeVersion exchangeVersion,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a validation context for EWS operations with specified scope
        /// </summary>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="scope">Validation scope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A configured validation context</returns>
        IEwsValidationContext CreateWithScope(
            ExchangeVersion exchangeVersion,
            string scope,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a validation context for extended property validation
        /// </summary>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A configured validation context for extended properties</returns>
        IEwsValidationContext CreateForExtendedProperties(
            ExchangeVersion exchangeVersion,
            CancellationToken cancellationToken = default);
    }
}