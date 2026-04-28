// PiQApi.Ews.Operations/Core/EwsCorrelationHeaderService.cs
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;

namespace PiQApi.Ews.Operations.Core
{
    /// <summary>
    /// Service for managing correlation headers in Exchange items
    /// </summary>
    public class EwsCorrelationHeaderService : IEwsCorrelationHeaderService
    {
        private readonly ILogger<EwsCorrelationHeaderService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsCorrelationHeaderService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public EwsCorrelationHeaderService(ILogger<EwsCorrelationHeaderService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Sets correlation header in an Exchange item
        /// </summary>
        public async Task SetCorrelationHeaderAsync(Item item, string correlationId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentException.ThrowIfNullOrEmpty(correlationId);

            try
            {
                // Define the extended property for the correlation ID header
                var correlationIdHeaderProperty = new ExtendedPropertyDefinition(
                    DefaultExtendedPropertySet.InternetHeaders,
                    "X-Correlation-ID",
                    MapiPropertyType.String);

                // Set the correlation ID as an extended property
                item.SetExtendedProperty(correlationIdHeaderProperty, correlationId);

                _logger.LogDebug("Added correlation header X-Correlation-ID:{CorrelationId} to item {ItemId}", 
                    correlationId, item.Id?.UniqueId ?? "new item");

                // Wait for the property to be applied
                await Task.CompletedTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set correlation header for item. CorrelationId: {CorrelationId}", correlationId);
                // We don't want to fail the operation just because we couldn't set the header
            }
        }
    }
}