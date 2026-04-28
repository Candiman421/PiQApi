// PiQApi.Ews.Core/Extensions/ServiceEndpointTypeExtensions.cs
using PiQApi.Abstractions.Enums;
//todo analyze if this should be moved up to operations or client layer
namespace PiQApi.Ews.Core.Extensions
{
    /// <summary>
    /// Extensions for ServiceEndpointType to provide service-specific naming
    /// </summary>
    public static class ServiceEndpointTypeExtensions
    {
        /// <summary>
        /// Gets the Exchange Web Services endpoint type
        /// </summary>
        public static ServiceEndpointType ExchangeWebServices => ServiceEndpointType.Primary;

        /// <summary>
        /// Gets the Graph endpoint type
        /// </summary>
        public static ServiceEndpointType Graph => ServiceEndpointType.Secondary;

        /// <summary>
        /// Gets the SharePoint endpoint type
        /// </summary>
        public static ServiceEndpointType SharePoint => ServiceEndpointType.Tertiary;

        /// <summary>
        /// Determines if the endpoint type is Exchange Web Services
        /// </summary>
        /// <param name="type">Endpoint type to check</param>
        /// <returns>True if the endpoint type is Exchange Web Services; otherwise, false</returns>
        public static bool IsExchangeWebServices(this ServiceEndpointType type) =>
            type == ServiceEndpointType.Primary;

        /// <summary>
        /// Determines if the endpoint type is Graph
        /// </summary>
        /// <param name="type">Endpoint type to check</param>
        /// <returns>True if the endpoint type is Graph; otherwise, false</returns>
        public static bool IsGraph(this ServiceEndpointType type) =>
            type == ServiceEndpointType.Secondary;

        /// <summary>
        /// Determines if the endpoint type is SharePoint
        /// </summary>
        /// <param name="type">Endpoint type to check</param>
        /// <returns>True if the endpoint type is SharePoint; otherwise, false</returns>
        public static bool IsSharePoint(this ServiceEndpointType type) =>
            type == ServiceEndpointType.Tertiary;
    }
}