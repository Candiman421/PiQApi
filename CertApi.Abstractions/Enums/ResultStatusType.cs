// CertApi.Abstractions/Enums/ResultStatusType.cs
namespace CertApi.Abstractions.Enums;

/// <summary>
/// Represents the outcome status of a completed operation
/// </summary>
public enum ResultStatusType
{
    /// <summary>
    /// Operation completed successfully
    /// </summary>
    Success = 0,

    /// <summary>
    /// Operation completed with partial success
    /// </summary>
    Partial = 1,

    /// <summary>
    /// Operation failed validation
    /// </summary>
    ValidationFailed = 2,

    /// <summary>
    /// Operation failed but in expected way
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Operation succeeded with warnings
    /// </summary>
    Warning = 4,

    /// <summary>
    /// Operation status could not be determined
    /// </summary>
    Unknown = 5,

    /// <summary>
    /// Resource was created (HTTP 201)
    /// </summary>
    Created = 6,

    /// <summary>
    /// Request was accepted (HTTP 202)
    /// </summary>
    Accepted = 7,

    /// <summary>
    /// Operation succeeded with no content (HTTP 204)
    /// </summary>
    NoContent = 8,

    /// <summary>
    /// Operation encountered an error (HTTP 500)
    /// </summary>
    Error = 9,

    /// <summary>
    /// Resource was not found (HTTP 404)
    /// </summary>
    NotFound = 10,

    /// <summary>
    /// Access to resource is forbidden (HTTP 403)
    /// </summary>
    Forbidden = 11,

    /// <summary>
    /// Authentication required (HTTP 401)
    /// </summary>
    Unauthorized = 12,

    /// <summary>
    /// Bad request parameters (HTTP 400)
    /// </summary>
    BadRequest = 13
}