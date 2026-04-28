// PiQApi.Abstractions/Validation/Models/PiQValidationResultBuilder.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;

namespace PiQApi.Abstractions.Validation.Models;

/// <summary>
/// Builder for creating validation results
/// </summary>
public class PiQValidationResultBuilder
{
    private bool _isValid = true;
    private readonly List<PiQValidationError> _errors = new();
    private Exception? _exception;
    private readonly Dictionary<string, object> _context = new();
    private string? _correlationId;
    private readonly IPiQValidationResultFactory _factory;

    /// <summary>
    /// Gets whether the builder has errors
    /// </summary>
    public bool HasErrors => _errors.Count > 0 || !_isValid;

    /// <summary>
    /// Creates a new validation result builder
    /// </summary>
    /// <param name="factory">Validation result factory</param>
    /// <param name="correlationId">Optional correlation ID</param>
    public PiQValidationResultBuilder(IPiQValidationResultFactory factory, string? correlationId = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _correlationId = correlationId;
    }

    /// <summary>
    /// Sets the correlation ID for the validation result
    /// </summary>
    public PiQValidationResultBuilder WithCorrelationId(string correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId, nameof(correlationId));
        _correlationId = correlationId;
        return this;
    }

    /// <summary>
    /// Sets the validation result to valid
    /// </summary>
    public PiQValidationResultBuilder Valid()
    {
        _isValid = true;
        return this;
    }

    /// <summary>
    /// Sets the validation result to invalid
    /// </summary>
    public PiQValidationResultBuilder Invalid()
    {
        _isValid = false;
        return this;
    }

    /// <summary>
    /// Adds an error to the validation result
    /// </summary>
    public PiQValidationResultBuilder WithError(string propertyName, string message, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        _errors.Add(new PiQValidationError(propertyName, message, errorCode, severity));
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
    public PiQValidationResultBuilder WithError(PiQValidationError error)
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
    public PiQValidationResultBuilder WithErrors(IEnumerable<PiQValidationError> errors)
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
    public PiQValidationResultBuilder WithException(Exception exception)
    {
        _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        _isValid = false;
        return this;
    }

    /// <summary>
    /// Adds a context value to the validation result
    /// </summary>
    public PiQValidationResultBuilder WithContextValue(string key, object value)
    {
        _context[key] = value;
        return this;
    }

    /// <summary>
    /// Adds an error to the validation result - alternative method name for compatibility
    /// </summary>
    public PiQValidationResultBuilder AddError(string propertyName, string message, string errorCode = "ValidationError", ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        return WithError(propertyName, message, errorCode, severity);
    }

    /// <summary>
    /// Adds a pre-existing validation error to the result
    /// </summary>
    /// <param name="error">The validation error to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public PiQValidationResultBuilder AddError(PiQValidationError error)
    {
        return WithError(error);
    }

    /// <summary>
    /// Adds a property error to the validation result
    /// </summary>
    public PiQValidationResultBuilder WithPropertyError(string errorCode, string propertyName, string message, ValidationSeverityType severity = ValidationSeverityType.Error)
    {
        return WithError(propertyName, message, errorCode, severity);
    }

    /// <summary>
    /// Merges another validation result
    /// </summary>
    public PiQValidationResultBuilder Merge(IPiQValidationResult other)
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
    public IPiQValidationResult Build()
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