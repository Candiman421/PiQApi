// CertApi.Ews.Core/Enums/EwsResponseCodeType.cs
namespace CertApi.Ews.Core.Enums
{
    /// <summary>
    /// Defines response codes specific to the Exchange Web Services operations
    /// </summary>
    public enum EwsResponseCodeType
    {
        /// <summary>
        /// Operation completed successfully
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Server is busy processing requests
        /// </summary>
        ErrorServerBusy,

        /// <summary>
        /// Resource not found
        /// </summary>
        ErrorItemNotFound,

        /// <summary>
        /// Access denied to requested resource
        /// </summary>
        ErrorAccessDenied,

        /// <summary>
        /// Invalid authorization token
        /// </summary>
        ErrorInvalidAuthorizationToken,

        /// <summary>
        /// Token expired
        /// </summary>
        ErrorAuthorizationTokenExpired,

        /// <summary>
        /// Network or connection error
        /// </summary>
        ErrorConnectionFailed,

        /// <summary>
        /// Throttling limit reached
        /// </summary>
        ErrorThrottled,

        /// <summary>
        /// Invalid operation
        /// </summary>
        ErrorInvalidOperation,

        /// <summary>
        /// Schema validation error
        /// </summary>
        ErrorSchemaValidation,

        /// <summary>
        /// Mailbox move in progress
        /// </summary>
        ErrorMailboxMoveInProgress,

        /// <summary>
        /// Data size exceeded limits
        /// </summary>
        ErrorSizeLimitExceeded,

        /// <summary>
        /// Unknown error
        /// </summary>
        ErrorUnknown,

        /// <summary>
        /// Internal server error
        /// </summary>
        ErrorInternalServerError,

        /// <summary>
        /// Mailbox unavailable
        /// </summary>
        ErrorMailboxUnavailable,

        /// <summary>
        /// Timeout occurred
        /// </summary>
        ErrorTimeout
    }
}
