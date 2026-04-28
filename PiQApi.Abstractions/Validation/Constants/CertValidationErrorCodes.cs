// PiQApi.Abstractions/Validation/Constants/CertValidationErrorCodes.cs
namespace PiQApi.Abstractions.Validation.Constants;

/// <summary>
/// Defines standard validation error codes used across the service
/// </summary>
public static class CertValidationErrorCodes
{
    /// <summary>
    /// Code for validation error
    /// </summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>
    /// Code for invalid operation
    /// </summary>
    public const string InvalidOperation = "INVALID_OPERATION";

    /// <summary>
    /// Code for invalid state
    /// </summary>
    public const string InvalidState = "INVALID_STATE";

    /// <summary>
    /// Code for not initialized
    /// </summary>
    public const string NotInitialized = "NOT_INITIALIZED";

    /// <summary>
    /// Code for service error
    /// </summary>
    public const string ServiceError = "SERVICE_ERROR";

    /// <summary>
    /// Code for authentication error
    /// </summary>
    public const string AuthenticationError = "AUTH_ERROR";

    /// <summary>
    /// Code for configuration error
    /// </summary>
    public const string ConfigurationError = "CONFIG_ERROR";

    #region Property Validation

    /// <summary>
    /// Code for required property
    /// </summary>
    public const string PropertyRequired = "PROPERTY_REQUIRED";

    /// <summary>
    /// Code for invalid property format
    /// </summary>
    public const string PropertyInvalidFormat = "PROPERTY_INVALID_FORMAT";

    /// <summary>
    /// Code for property too short
    /// </summary>
    public const string PropertyTooShort = "PROPERTY_TOO_SHORT";

    /// <summary>
    /// Code for property too long
    /// </summary>
    public const string PropertyTooLong = "PROPERTY_TOO_LONG";

    /// <summary>
    /// Code for property out of range
    /// </summary>
    public const string PropertyOutOfRange = "PROPERTY_OUT_OF_RANGE";

    /// <summary>
    /// Code for duplicate property
    /// </summary>
    public const string PropertyDuplicate = "PROPERTY_DUPLICATE";

    #endregion

    #region Entity Validation

    /// <summary>
    /// Code for entity not found
    /// </summary>
    public const string EntityNotFound = "ENTITY_NOT_FOUND";

    /// <summary>
    /// Code for entity already exists
    /// </summary>
    public const string EntityAlreadyExists = "ENTITY_ALREADY_EXISTS";

    /// <summary>
    /// Code for entity invalid state
    /// </summary>
    public const string EntityInvalidState = "ENTITY_INVALID_STATE";

    #endregion

    #region Resource Validation

    /// <summary>
    /// Code for resource not found
    /// </summary>
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";

    /// <summary>
    /// Code for resource locked
    /// </summary>
    public const string ResourceLocked = "RESOURCE_LOCKED";

    /// <summary>
    /// Code for resource quota exceeded
    /// </summary>
    public const string ResourceQuotaExceeded = "RESOURCE_QUOTA_EXCEEDED";

    #endregion
}
