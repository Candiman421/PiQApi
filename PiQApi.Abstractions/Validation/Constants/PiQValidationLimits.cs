// PiQApi.Abstractions/Validation/Constants/PiQValidationLimits.cs
namespace PiQApi.Abstractions.Validation.Constants;

/// <summary>
/// Defines size and quantity limits for validation across the service
/// </summary>
public static class PiQValidationLimits
{
    /// <summary>
    /// Maximum length for strings in general
    /// </summary>
    public const int MaxStringLength = 4000;

    /// <summary>
    /// Minimum length for strings in general
    /// </summary>
    public const int MinStringLength = 1;

    /// <summary>
    /// Maximum size for collections
    /// </summary>
    public const int MaxCollectionSize = 1000;

    /// <summary>
    /// Maximum nesting depth for validation
    /// </summary>
    public const int MaxDepth = 5;

    /// <summary>
    /// Default timeout in seconds
    /// </summary>
    public const int DefaultTimeoutSeconds = 30;

    #region Mail Limits

    /// <summary>
    /// Maximum length for email subject
    /// </summary>
    public const int MaxMailSubjectLength = 255;

    /// <summary>
    /// Maximum length for email body
    /// </summary>
    public const int MaxMailBodyLength = 1000000;

    /// <summary>
    /// Maximum number of recipients per email
    /// </summary>
    public const int MaxMailRecipients = 500;

    /// <summary>
    /// Maximum number of attachments per email
    /// </summary>
    public const int MaxMailAttachments = 100;

    /// <summary>
    /// Maximum size for email attachments (25MB)
    /// </summary>
    public const int MaxMailAttachmentSize = 25 * 1024 * 1024;

    #endregion

    #region Authentication Limits

    /// <summary>
    /// Minimum length for client ID
    /// </summary>
    public const int MinClientIdLength = 10;

    /// <summary>
    /// Maximum length for client ID
    /// </summary>
    public const int MaxClientIdLength = 50;

    /// <summary>
    /// Minimum length for client secret
    /// </summary>
    public const int MinSecretLength = 16;

    /// <summary>
    /// Maximum length for client secret
    /// </summary>
    public const int MaxSecretLength = 128;

    /// <summary>
    /// Minimum number of OAuth scopes
    /// </summary>
    public const int MinScopeCount = 1;

    /// <summary>
    /// Maximum number of OAuth scopes
    /// </summary>
    public const int MaxScopeCount = 10;

    #endregion

    #region Calendar Limits

    /// <summary>
    /// Maximum number of attendees per meeting
    /// </summary>
    public const int MaxCalendarAttendees = 1000;

    /// <summary>
    /// Maximum number of recurrences
    /// </summary>
    public const int MaxCalendarRecurrences = 999;

    /// <summary>
    /// Maximum length for appointment subject
    /// </summary>
    public const int MaxCalendarSubjectLength = 255;

    /// <summary>
    /// Maximum length for location
    /// </summary>
    public const int MaxCalendarLocationLength = 255;

    #endregion

    #region Header Limits

    /// <summary>
    /// Maximum number of headers
    /// </summary>
    public const int MaxHeaderCount = 50;

    /// <summary>
    /// Maximum length for header name
    /// </summary>
    public const int MaxHeaderNameLength = 50;

    /// <summary>
    /// Maximum length for header value
    /// </summary>
    public const int MaxHeaderValueLength = 1000;

    #endregion

    #region Property Limits

    /// <summary>
    /// Maximum number of properties
    /// </summary>
    public const int MaxPropertyCount = 100;

    /// <summary>
    /// Maximum number of extended properties
    /// </summary>
    public const int MaxExtendedPropertyCount = 20;

    /// <summary>
    /// Maximum length for custom property name
    /// </summary>
    public const int MaxCustomPropertyNameLength = 255;

    #endregion
}
