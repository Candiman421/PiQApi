// CertApi.Abstractions/Service/ICertErrorMappingService.cs
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Service;

/// <summary>
/// Service for mapping service errors to application errors
/// </summary>
public interface ICertErrorMappingService
{
    /// <summary>
    /// Maps a service error to an ErrorCodeType
    /// </summary>
    /// <param name="serviceError">Service error to map</param>
    /// <returns>Mapped error code</returns>
    ErrorCodeType MapServiceErrorToErrorCodeType(ErrorCodeType serviceError);

    /// <summary>
    /// Creates a service exception from a response exception
    /// </summary>
    /// <param name="responseException">Response exception to map</param>
    /// <returns>Service exception</returns>
    Exception CreateServiceExceptionFromResponse(Exception responseException);

    /// <summary>
    /// Maps a system exception to an ErrorCodeType
    /// </summary>
    /// <param name="exception">Exception to map</param>
    /// <returns>Mapped error code</returns>
    ErrorCodeType MapSystemExceptionToErrorCodeType(Exception exception);
}