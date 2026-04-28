// PiQApi.Abstractions/Enums/ErrorCodeType.cs
namespace PiQApi.Abstractions.Enums;

/// <summary>
/// Represents error code types used throughout the system
/// </summary>
public enum ErrorCodeType
{
    /// <summary>
    /// No error occurred
    /// </summary>
    None = 0,

    /// <summary>
    /// Authentication failure
    /// </summary>
    AuthenticationFailure = 1,

    /// <summary>
    /// Authentication error
    /// </summary>
    AuthenticationError = 2,

    /// <summary>
    /// Connection error
    /// </summary>
    ConnectionError = 3,

    /// <summary>
    /// Resource not found
    /// </summary>
    NotFound = 4,

    /// <summary>
    /// Validation error
    /// </summary>
    ValidationError = 5,

    /// <summary>
    /// Unauthorized access
    /// </summary>
    Unauthorized = 6,

    /// <summary>
    /// Access denied
    /// </summary>
    AccessDenied = 7,

    /// <summary>
    /// Invalid operation
    /// </summary>
    InvalidOperation = 8,

    /// <summary>
    /// Internal server error
    /// </summary>
    InternalServerError = 9,

    /// <summary>
    /// Service unavailable
    /// </summary>
    ServiceUnavailable = 10,

    /// <summary>
    /// Configuration error
    /// </summary>
    ConfigurationError = 11,

    /// <summary>
    /// Timeout error
    /// </summary>
    TimeoutError = 12,

    /// <summary>
    /// Rate limit exceeded
    /// </summary>
    RateLimitExceeded = 13,

    /// <summary>
    /// Unknown error
    /// </summary>
    Unknown = 14,

    /// <summary>
    /// Generic service error
    /// </summary>
    ServiceError = 15,

    /// <summary>
    /// Connection failed
    /// </summary>
    ConnectionFailed = 16,

    /// <summary>
    /// Timeout expired
    /// </summary>
    TimeoutExpired = 17,

    /// <summary>
    /// Item not found
    /// </summary>
    ItemNotFound = 18,

    /// <summary>
    /// No error (alternative to None)
    /// </summary>
    NoError = 19,

    /// <summary>
    /// Server busy
    /// </summary>
    ServerBusy = 20,

    /// <summary>
    /// Too many requests
    /// </summary>
    TooManyRequests = 21,

    /// <summary>
    /// Invalid request
    /// </summary>
    InvalidRequest = 22,

    /// <summary>
    /// Bad request parameters (HTTP 400)
    /// </summary>
    BadRequest = 23,

    /// <summary>
    /// Resource not found (HTTP 404)
    /// </summary>
    ResourceNotFound = 24,

    /// <summary>
    /// Resource conflict (HTTP 409)
    /// </summary>
    ResourceConflict = 25,

    /// <summary>
    /// Conflict (HTTP 409)
    /// </summary>
    Conflict = 26,

    /// <summary>
    /// Generic error type
    /// </summary>
    Error = 27,

    /// <summary>
    /// Request timeout (HTTP 408)
    /// </summary>
    Timeout = 28,
    
    /// <summary>
    /// Security violation or breach
    /// </summary>
    SecurityViolation = 29,
    
    /// <summary>
    /// Data corruption or integrity error
    /// </summary>
    DataCorruption = 30,
    
    /// <summary>
    /// Specific error for throttling with retry information
    /// </summary>
    ThrottlingError = 31,
    
    /// <summary>
    /// Error related to certificate validation or processing
    /// </summary>
    CertificateError = 32
}