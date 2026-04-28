// PiQApi.Abstractions/Validation/Constants/CertValidationMessages.cs
namespace PiQApi.Abstractions.Validation.Constants;

/// <summary>
/// Defines standard validation messages used across the service
/// </summary>
public static class CertValidationMessages
{
    /// <summary>
    /// Message for validation failure
    /// </summary>
    public const string ValidationFailed = "Validation failed";

    /// <summary>
    /// Message for successful validation
    /// </summary>
    public const string ValidationSuccessful = "Validation completed successfully";

    /// <summary>
    /// Message for invalid operation
    /// </summary>
    public const string InvalidOperation = "Invalid operation";

    /// <summary>
    /// Message for invalid state
    /// </summary>
    public const string InvalidState = "Object is in an invalid state";

    /// <summary>
    /// Message for uninitialized service
    /// </summary>
    public const string NotInitialized = "Service is not initialized";

    /// <summary>
    /// Message for already initialized service
    /// </summary>
    public const string AlreadyInitialized = "Service is already initialized";

    /// <summary>
    /// Message for service not ready
    /// </summary>
    public const string ServiceNotReady = "Service is not ready";

    /// <summary>
    /// Message for disposed object
    /// </summary>
    public const string ObjectDisposed = "Object has been disposed";

    /// <summary>
    /// Message for validation timeout
    /// </summary>
    public const string ValidationTimeout = "Validation operation timed out";

    /// <summary>
    /// Format for property validation failure message
    /// </summary>
    public const string PropertyValidationFailed = "Property validation failed: {0}";

    /// <summary>
    /// Message for multiple validation errors
    /// </summary>
    public const string MultipleValidationErrors = "Multiple validation errors occurred";

    /// <summary>
    /// Format for validation error message
    /// </summary>
    public const string ValidationErrorFormat = "{0}: {1}";

    /// <summary>
    /// Format for property error message
    /// </summary>
    public const string PropertyErrorFormat = "[{0}] {1}";

    #region Severity Messages

    /// <summary>
    /// Format for error severity message
    /// </summary>
    public const string ErrorSeverityFormat = "{0} error(s)";

    /// <summary>
    /// Format for warning severity message
    /// </summary>
    public const string WarningSeverityFormat = "{0} warning(s)";

    /// <summary>
    /// Format for info severity message
    /// </summary>
    public const string InfoSeverityFormat = "{0} info message(s)";

    /// <summary>
    /// Display name for error severity
    /// </summary>
    public const string ErrorSeverityDisplay = "ErrorInfo";

    /// <summary>
    /// Display name for warning severity
    /// </summary>
    public const string WarningSeverityDisplay = "Warning";

    /// <summary>
    /// Display name for info severity
    /// </summary>
    public const string InfoSeverityDisplay = "Information";

    #endregion

    #region State Messages

    /// <summary>
    /// Message for validation in progress
    /// </summary>
    public const string StateInProgress = "Validation in progress";

    /// <summary>
    /// Message for validation complete
    /// </summary>
    public const string StateComplete = "Validation complete";

    /// <summary>
    /// Message for validation failed
    /// </summary>
    public const string StateFailed = "Validation failed";

    /// <summary>
    /// Message for validation cancelled
    /// </summary>
    public const string StateCancelled = "Validation cancelled";

    #endregion

    #region Aggregation Messages

    /// <summary>
    /// Format for summary message
    /// </summary>
    public const string SummaryFormat = "{0} total issues ({1} errors, {2} warnings, {3} info)";

    /// <summary>
    /// Message for no issues found
    /// </summary>
    public const string NoIssuesFound = "No validation issues found";

    /// <summary>
    /// Format for group header
    /// </summary>
    public const string GroupHeaderFormat = "{0}: {1} {2}";

    /// <summary>
    /// Text for single issue
    /// </summary>
    public const string SingleIssue = "issue";

    /// <summary>
    /// Text for multiple issues
    /// </summary>
    public const string MultipleIssues = "issues";

    /// <summary>
    /// Prefix for list items
    /// </summary>
    public const string ListItemPrefix = "- ";

    #endregion
}
