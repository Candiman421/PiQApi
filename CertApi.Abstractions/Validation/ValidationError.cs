// CertApi.Abstractions/Validation/ValidationError.cs
using CertApi.Abstractions.Enums;
using System.Diagnostics;

namespace CertApi.Abstractions.Validation
{
    [DebuggerDisplay("{Message} (Property: {PropertyName}, Severity: {Severity})")]
    public class ValidationError : IEquatable<ValidationError>
    {
        public string? PropertyName { get; }
        public string Message { get; }
        public ValidationSeverityType Severity { get; }

        // Unique identifier to help track error origins
        public Guid ErrorId { get; } = Guid.NewGuid();
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public ValidationError(string message, ValidationSeverityType severity = ValidationSeverityType.Error)
            : this(null, message, severity)
        {
        }

        public ValidationError(string? propertyName, string message, ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            PropertyName = propertyName;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Severity = severity;

            Debug.WriteLine($"ValidationError created: ID={ErrorId}, Property={PropertyName}, Message={Message}, Severity={Severity}");
        }

        public bool Equals(ValidationError? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            bool result = string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal) &&
                          string.Equals(Message, other.Message, StringComparison.Ordinal) &&
                          Severity == other.Severity;

            Debug.WriteLine($"ValidationError Equality Check: {result} (Self ID: {ErrorId}, Other ID: {other.ErrorId})");
            return result;
        }

        public override bool Equals(object? obj) =>
            obj is ValidationError other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(PropertyName, Message, Severity);

        public static bool operator ==(ValidationError? left, ValidationError? right) =>
            Equals(left, right);

        public static bool operator !=(ValidationError? left, ValidationError? right) =>
            !Equals(left, right);

        public override string ToString()
        {
            // Modify to match test expectations when no property name exists
            return string.IsNullOrEmpty(PropertyName)
                ? $"{Message} ({Severity})"  // Changed from "[Global]"
                : $"[{PropertyName}] {Message} ({Severity})";
        }
    }
}