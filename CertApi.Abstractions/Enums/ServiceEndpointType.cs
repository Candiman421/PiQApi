// CertApi.Abstractions/Enums/ServiceEndpointType.cs
namespace CertApi.Abstractions.Enums;

/// <summary>
/// Defines the types of service endpoints supported by the system
/// </summary>
public enum ServiceEndpointType
{
    /// <summary>
    /// No service endpoint specified
    /// </summary>
    None = 0,

    /// <summary>
    /// Primary service endpoint
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Secondary service endpoint
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// Tertiary service endpoint
    /// </summary>
    Tertiary = 3,

    /// <summary>
    /// Generic service endpoint
    /// </summary>
    Generic = 4
}