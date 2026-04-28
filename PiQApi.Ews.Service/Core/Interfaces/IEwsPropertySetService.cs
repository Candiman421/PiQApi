// PiQApi.Ews.Service/Core/Interfaces/IEwsPropertySetService.cs
using Microsoft.Exchange.WebServices.Data;

namespace PiQApi.Ews.Service.Core.Interfaces
{
    /// <summary>
    /// Interface for managing Exchange property sets
    /// </summary>
    public interface IEwsPropertySetService
    {
        /// <summary>
        /// Creates a property set with email properties
        /// </summary>
        /// <param name="basePropertySet">Optional base property set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Property set with email properties</returns>
        Task<PropertySet> WithEmailPropertiesAsync(
            PropertySet? basePropertySet = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a property set with folder properties
        /// </summary>
        /// <param name="basePropertySet">Optional base property set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Property set with folder properties</returns>
        Task<PropertySet> WithFolderPropertiesAsync(
            PropertySet? basePropertySet = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a property set with calendar properties
        /// </summary>
        /// <param name="basePropertySet">Optional base property set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Property set with calendar properties</returns>
        Task<PropertySet> WithCalendarPropertiesAsync(
            PropertySet? basePropertySet = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a property set with extended properties
        /// </summary>
        /// <param name="basePropertySet">Optional base property set</param>
        /// <param name="extendedProperties">Extended properties to include</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Property set with extended properties</returns>
        Task<PropertySet> WithExtendedPropertiesAsync(
            PropertySet? basePropertySet,
            IEnumerable<ExtendedPropertyDefinition> extendedProperties,
            CancellationToken cancellationToken = default);
    }
}