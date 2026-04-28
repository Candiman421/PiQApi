// PiQApi.Ews.Operations/Core/Interfaces/IEwsCorrelationHeaderService.cs
using Microsoft.Exchange.WebServices.Data;

namespace PiQApi.Ews.Operations.Core.Interfaces
{
    /// <summary>
    /// Service for managing correlation headers in Exchange items
    /// </summary>
    public interface IEwsCorrelationHeaderService
    {
        /// <summary>
        /// Sets correlation header in an Exchange item
        /// </summary>
        /// <param name="item">Item to update</param>
        /// <param name="correlationId">Correlation ID to set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SetCorrelationHeaderAsync(Item item, string correlationId, CancellationToken cancellationToken);
    }
}