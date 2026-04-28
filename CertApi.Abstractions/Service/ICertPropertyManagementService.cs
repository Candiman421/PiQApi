// CertApi.Abstractions/Service/ICertPropertyManagementService.cs
namespace CertApi.Abstractions.Service;

/// <summary>
/// Service for managing property sets for service requests
/// </summary>
public interface ICertPropertyManagementService
{
    /// <summary>
    /// Creates a property set with specified properties
    /// </summary>
    /// <param name="basePropertySet">Base property set to extend</param>
    /// <param name="properties">Properties to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extended property set</returns>
    Task<object> WithPropertiesAsync(object basePropertySet, IEnumerable<string> properties, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a property set with extended properties
    /// </summary>
    /// <param name="basePropertySet">Base property set to extend</param>
    /// <param name="extendedProperties">Extended properties to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Property set with extended properties</returns>
    Task<object> WithExtendedPropertiesAsync(object basePropertySet, IEnumerable<object> extendedProperties, CancellationToken cancellationToken = default);
}