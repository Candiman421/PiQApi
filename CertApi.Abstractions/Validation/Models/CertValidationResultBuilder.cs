// CertApi.Abstractions/Validation/Models/CertValidationResultBuilder.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Factories;

namespace CertApi.Abstractions.Validation.Models;

/// <summary>
/// Builder for creating validation results
/// </summary>
public class CertValidationResultBuilder
{
    private bool _isValid = true;
    private readonly List<CertValidationError> _errors = new();
    private Exception? _exception;
    private readonly Dictionary<string, object> _context = new();
    private string? _correlationId;
    private readonly ICertValidationResultFactory _factory;

    /// <summary>
    /// Gets whether the builder has errors
    /// </summary>
    public bool HasErrors => _errors.Count > 0 || !_isValid;

    /// <summary>
    /// Creates a new validation result builder
    /// </summary>
    /// <param name="factory">Validation result factory</param>
    /// <param name="correlationId">Optional correlation ID</param>
    public CertValidationResultBuilder(ICertValidationResultFactory factory, string? correlationId = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _correlationId = correlationId;
    }

    /// <summary>
    /// Sets the correlation ID for the validation result
    /// </summary>
    public CertValidationResultBuilder WithCorrelationId(string correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId, nameof(correlationId));
        _correlationId = correlationId;
        return this;
    }

    /// <summary>
    /// Sets the validation result to valid
    /// </summary>
    public CertValidationResultBuilder Valid()
    {
        _isValid = true;
        return this;
    }

    /// <summary>
    /// Sets the validation result to invalid
    /// </summary>
    public CertValidationResultBuilder Invalid()
    {
        _isValid = false;
        return this;
    }

    /// <summary>
    /// Adds an error to the validation result
    /// </summary>
    public CertValidationResultBuilder WithError(string propertyName, string message, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        _errors.Add(new CertValidationError(propertyName, message, errorCode, severity));
        if (severity == ValidationSeverityType.Error)
        {
            _isValid = false;
        }
        return this;
    }

    /// <summary>
    /// Adds a pre-existing validation error to the result
    /// </summary>
    /// <param name="error">The validation error to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public CertValidationResultBuilder WithError(CertValidationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _errors.Add(error);
        if (error.Severity == ValidationSeverityType.Error)
        {
            _isValid = false;
        }
        return this;
    }

    /// <summary>
    /// Adds multiple errors to the validation result
    /// </summary>
    public CertValidationResultBuilder WithErrors(IEnumerable<CertValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        foreach (var error in errors)
        {
            _errors.Add(error);
            if (error.Severity == ValidationSeverityType.Error)
            {
                _isValid = false;
            }
        }
        return this;
    }

    /// <summary>
    /// Sets the exception that caused validation to fail
    /// </summary>
    public CertValidationResultBuilder WithException(Exception exception)
    {
        _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        _isValid = false;
        return this;
    }

    /// <summary>
    /// Adds a context value to the validation result
    /// </summary>
    public CertValidationResultBuilder WithContextValue(string key, object value)
    {
        _context[key] = value;
        return this;
    }

    /// <summary>
    /// Adds an error to the validation result - alternative method name for compatibility
    /// </summary>
    public CertValidationResultBuilder AddError(string propertyName, string message, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        return WithError(propertyName, message, errorCode, severity);
    }

    /// <summary>
    /// Adds a pre-existing validation error to the result
    /// </summary>
    /// <param name="error">The validation error to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public CertValidationResultBuilder AddError(CertValidationError error)
    {
        return WithError(error);
    }

    /// <summary>
    /// Adds a property error to the validation result
    /// </summary>
    public CertValidationResultBuilder WithPropertyError(string errorCode, string propertyName, string message, ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        return WithError(propertyName, message, errorCode, severity);
    }

    /// <summary>
    /// Merges another validation result
    /// </summary>
    public CertValidationResultBuilder Merge(ICertValidationResult other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other.Errors.Count > 0)
        {
            WithErrors(other.Errors);
        }

        if (other.Exception != null)
        {
            WithException(other.Exception);
        }

        foreach (var kvp in other.Context)
        {
            WithContextValue(kvp.Key, kvp.Value);
        }

        return this;
    }

    /// <summary>
    /// Builds the validation result
    /// </summary>
    public ICertValidationResult Build()
    {
        // Add correlation ID to context if present
        if (!string.IsNullOrEmpty(_correlationId) && !_context.ContainsKey("CorrelationId"))
        {
            _context["CorrelationId"] = _correlationId;
        }

        // Use _isValid flag to allow explicit invalidation without errors
        if (!_isValid && _errors.Count == 0 && _exception == null)
        {
            // Explicitly marked invalid but no specific errors
            return _factory.FromError("ValidationState", "Validation failed", "InvalidState");
        }

        if (_errors.Count > 0)
        {
            var result = _factory.FromErrors(_errors);

            // Add exception to context if present
            if (_exception != null)
            {
                _context["Exception"] = _exception;
            }

            return result;
        }
        else if (_exception != null)
        {
            return _factory.FromException(_exception);
        }
        else
        {
            return _factory.Success();
        }
    }
}