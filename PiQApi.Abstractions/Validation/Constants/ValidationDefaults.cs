// PiQApi.Abstractions/Validation/Constants/ValidationDefaults.cs
namespace PiQApi.Abstractions.Validation.Constants
{
    /// <summary>
    /// Defines default values and constraints for validation across the service
    /// </summary>
    public static class ValidationDefaults
    {
        // Suppressing the nested type warning as these types are part of a well-defined validation framework
        // and are used throughout the codebase as constants containers
#pragma warning disable CA1034 // Nested types should not be visible

        public static class CommonDefaults
        {
            public const int MaxStringLength = 4000;
            public const int MinStringLength = 1;
            public const int MaxCollectionSize = 1000;
            public const int MaxDepth = 5;
            public const int DefaultTimeoutSeconds = 30;
        }

        public static class MailDefaults
        {
            public const int MaxSubjectLength = 255;
            public const int MaxBodyLength = 1000000;
            public const int MaxRecipients = 500;
            public const int MaxAttachments = 100;
            public const int MaxAttachmentSize = 25 * 1024 * 1024; // 25MB
            public static readonly string[] DefaultAllowedExtensions = { ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".jpg", ".jpeg", ".png", ".gif" };
        }

        public static class AuthenticationDefaults
        {
            public const int MinClientIdLength = 10;
            public const int MaxClientIdLength = 50;
            public const int MinSecretLength = 16;
            public const int MaxSecretLength = 128;
            public const int MinScopeCount = 1;
            public const int MaxScopeCount = 10;
        }

        public static class CalendarDefaults
        {
            public const int MaxAttendeesPerMeeting = 1000;
            public const int MaxRecurrenceCount = 999;
            public const int MaxAppointmentSubjectLength = 255;
            public const int MaxLocationLength = 255;
        }

        public static class CertHeaders
        {
            public const int MaxHeaderCount = 50;
            public const int MaxHeaderNameLength = 50;
            public const int MaxHeaderValueLength = 1000;
        }

        public static class PropertySetDefaults
        {
            public const int MaxPropertyCount = 100;
            public const int MaxExtendedPropertyCount = 20;
            public const int MaxCustomPropertyNameLength = 255;
        }

        public static class MessageDefaults
        {
            public const string ValidationFailed = "Validation failed";
            public const string ValidationSuccessful = "Validation completed successfully";
            public const string InvalidOperation = "Invalid operation";
            public const string InvalidState = "Object is in an invalid state";
            public const string NotInitialized = "Service is not initialized";
            public const string AlreadyInitialized = "Service is already initialized";
            public const string ServiceNotReady = "Service is not ready";
            public const string ObjectDisposed = "Object has been disposed";
            public const string ValidationTimeout = "Validation operation timed out";
            public const string PropertyValidationFailed = "Property validation failed: {0}";
            public const string MultipleValidationErrors = "Multiple validation errors occurred";
            public const string ValidationErrorFormat = "{0}: {1}";
            public const string PropertyErrorFormat = "[{0}] {1}";
        }

        public static class ErrorCodeDefaults
        {
            public const string ValidationError = "VALIDATION_ERROR";
            public const string InvalidOperation = "INVALID_OPERATION";
            public const string InvalidState = "INVALID_STATE";
            public const string NotInitialized = "NOT_INITIALIZED";
            public const string ServiceError = "SERVICE_ERROR";
            public const string AuthenticationError = "AUTH_ERROR";
            public const string ConfigurationError = "CONFIG_ERROR";
        }

        public static class ModeDefaults
        {
            public const string Strict = "Strict";
            public const string Lenient = "Lenient";
            public const string Debug = "Debug";
        }

        public static class SeverityDefaults
        {
            public const string ErrorSeverityFormat = "{0} error(s)";
            public const string WarningSeverityFormat = "{0} warning(s)";
            public const string InfoSeverityFormat = "{0} info message(s)";

            public static class DisplayDefaults
            {
                public const string Error = "Error";
                public const string Warning = "Warning";
                public const string Info = "Information";
            }

            public static class DescriptionDefaults
            {
                public const string Error = "Operation cannot proceed";
                public const string Warning = "Operation can proceed with caution";
                public const string Info = "Additional information available";
            }
        }

        public static class StateDefaults
        {
            public const string InProgress = "Validation in progress";
            public const string Complete = "Validation complete";
            public const string Failed = "Validation failed";
            public const string Cancelled = "Validation cancelled";
        }

        public static class AggregationDefaults
        {
            public const string SummaryFormat = "{0} total issues ({1} errors, {2} warnings, {3} info)";
            public const string NoIssuesFound = "No validation issues found";
            public const string GroupHeaderFormat = "{0}: {1} {2}";
            public const string SingleIssue = "issue";
            public const string MultipleIssues = "issues";
            public const string ListItemPrefix = "- ";
        }

        public static class ValidationErrorSeverityDefaults
        {
            public const int Error = 0;
            public const int Warning = 1;
            public const int Info = 2;
        }

#pragma warning restore CA1034 // Nested types should not be visible
    }
}