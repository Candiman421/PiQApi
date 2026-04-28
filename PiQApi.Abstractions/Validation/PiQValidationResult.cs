// PiQApi.Abstractions/Validation/PiQValidationResult.cs
using System.Diagnostics;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;
using PiQApi.Abstractions.Service.Interfaces;
using PiQApi.Abstractions.Validation.Constants;

namespace PiQApi.Abstractions.Validation
{
    public sealed class PiQValidationResult
    {
        private readonly List<ValidationError> _errors = new();

        public bool HasErrors => _errors.Count > 0;
        public bool IsValid => !HasErrors;

        public bool HasErrorsOfSeverity(ValidationSeverityType severity) =>
            _errors.Any(e => e.Severity == severity);

        public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

        public IEnumerable<ValidationError> GetErrorsBySeverity(ValidationSeverityType severity)
        {
            Debug.WriteLine($"Retrieving errors with severity: {severity}");
            var filteredErrors = _errors.Where(e => e.Severity == severity).ToList();
            Debug.WriteLine($"Found {filteredErrors.Count} errors with severity {severity}");
            return filteredErrors;
        }

        public string GetErrorSummary()
        {
            if (IsValid)
            {
                Debug.WriteLine("Validation successful, returning default success message");
                return ValidationDefaults.MessageDefaults.ValidationSuccessful;
            }

            var errorGroups = _errors
                .GroupBy(e => e.Severity)
                .OrderBy(g => g.Key)
                .ToList();

            var summary = new List<string>();
            foreach (var group in errorGroups)
            {
                var errors = group.ToList();
                summary.Add($"{group.Key}: {errors.Count} {(errors.Count == 1 ? "issue" : "issues")}");
                foreach (var error in errors)
                {
                    summary.Add($"- {error.Message}");
                }
            }

            var finalSummary = string.Join(Environment.NewLine, summary);
            Debug.WriteLine($"Generated error summary: {finalSummary}");
            return finalSummary;
        }

        public IServiceResponse<T> ToServiceResponse<T>(T? result = default, string? correlationId = null) where T : class
        {
            Debug.WriteLine($"Converting validation result to service response. Errors: {_errors.Count}");

            if (IsValid)
            {
                Debug.WriteLine("Validation successful, creating success response");
                return new ServiceResponse<T>(
                    result,
                    ServiceResultStatusType.Success,
                    ErrorCodeType.None,
                    string.Empty,
                    correlationId ?? string.Empty);
            }

            var highestSeverity = _errors.Max(e => e.Severity);
            var status = highestSeverity switch
            {
                ValidationSeverityType.Error => ServiceResultStatusType.ValidationFailed,
                ValidationSeverityType.Warning => ServiceResultStatusType.Partial,
                ValidationSeverityType.Info => ServiceResultStatusType.Success,
                _ => ServiceResultStatusType.ValidationFailed
            };

            var errorMessages = string.Join(Environment.NewLine, _errors.Select(e => e.Message));

            Debug.WriteLine($"Creating error response. Status: {status}, Error Count: {_errors.Count}");

            return new ServiceResponse<T>(
                result,
                status,
                ErrorCodeType.ValidationError,
                errorMessages,
                correlationId ?? string.Empty);
        }

        private PiQValidationResult()
        {
            Debug.WriteLine($"PiQValidationResult created: {GetHashCode()}");
        }

        public static PiQValidationResult Success()
        {
            var result = new PiQValidationResult();
            Debug.WriteLine($"Created Success Result: {result.GetHashCode()}");
            return result;
        }

        public static PiQValidationResult Error(string message, ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            var result = new PiQValidationResult();
            result.AddError(message, severity);
            return result;
        }

        public static PiQValidationResult FromErrors(IEnumerable<ValidationError> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            var result = new PiQValidationResult();

            foreach (var error in errors)
            {
                result.AddError(error.PropertyName, error.Message, error.Severity);
            }

            Debug.WriteLine($"FromErrors Result: {result.GetHashCode()}, Errors: {result.Errors.Count}");
            return result;
        }

        public void AddError(string message, ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            var error = new ValidationError(message, severity);
            _errors.Add(error);
            Debug.WriteLine($"Added General Error: {error.Message}, Current Error Count: {_errors.Count}");
        }

        public void AddError(string? propertyName, string message, ValidationSeverityType severity = ValidationSeverityType.Error)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);

            // Create a new error instance
            var newError = new ValidationError(propertyName, message, severity);

            // Add to the list (without duplicate checking)
            _errors.Add(newError);

            Console.WriteLine($"Added error: {message} (Property: {propertyName ?? "N/A"}, Severity: {severity})");
        }

        public void Merge(PiQValidationResult other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (other.Errors.Count == 0)
                return;

            // Log diagnostic info
            Console.WriteLine($"Merging results. Current errors: {_errors.Count}, Incoming errors: {other.Errors.Count}");

            // Process each incoming error
            foreach (var error in other.Errors)
            {
                Console.WriteLine($"Attempting to add error: {error.Message} (Property: {error.PropertyName ?? "N/A"})");

                // Check if an equivalent error already exists
                bool isDuplicate = _errors.Any(existingError =>
                    string.Equals(existingError.Message, error.Message, StringComparison.Ordinal) &&
                    string.Equals(existingError.PropertyName, error.PropertyName, StringComparison.Ordinal) &&
                    existingError.Severity == error.Severity);

                // If error validation context expects to aggregate all errors or not a duplicate, add it
                if (!isDuplicate)
                {
                    // Create a new error to avoid reference sharing
                    var newError = new ValidationError(
                        error.PropertyName,
                        error.Message,
                        error.Severity
                    );

                    // Add to errors list
                    _errors.Add(newError);

                    Console.WriteLine($"Added error: {error.Message}");
                }
                else
                {
                    Console.WriteLine($"Skipping duplicate error: {error.Message}");
                }
            }

            Console.WriteLine($"Merge complete. Total errors: {_errors.Count}");
        }
    }
}