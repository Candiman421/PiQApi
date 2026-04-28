// PiQApi.Core/Exceptions/Infrastructure/PiQConfigurationException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when configuration validation fails
/// </summary>
public sealed class PiQConfigurationException : PiQServiceException
{
    private readonly List<string> _validationErrors = new();

    /// <summary>
    /// Gets the collection of validation error messages
    /// </summary>
    public IReadOnlyCollection<string> ValidationErrors => _validationErrors.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConfigurationException"/> class
    /// </summary>
    public PiQConfigurationException()
        : base("A configuration error occurred", "ConfigurationError")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConfigurationException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PiQConfigurationException(string message)
        : base(message, "ConfigurationError")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConfigurationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQConfigurationException(string message, Exception? inner)
        : base(message, "ConfigurationError", inner)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConfigurationException"/> class with a specified error message
    /// and error code
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    public PiQConfigurationException(string message, string? errorCode)
        : base(message, errorCode ?? "ConfigurationError")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConfigurationException"/> class with a specified error message,
    /// error code, and collection of validation error messages
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    /// <param name="errors">The collection of validation error messages</param>
    public PiQConfigurationException(string message, string? errorCode, IEnumerable<string>? errors)
        : base(message, errorCode ?? "ConfigurationError")
    {
        if (errors != null)
        {
            _validationErrors.AddRange(errors);
            AddData(nameof(ValidationErrors), _validationErrors);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQConfigurationException"/> class with a specified error message,
    /// error code, collection of validation error messages, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    /// <param name="errors">The collection of validation error messages</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQConfigurationException(string message, string? errorCode, IEnumerable<string>? errors, Exception? inner)
        : base(message, errorCode ?? "ConfigurationError", inner)
    {
        if (errors != null)
        {
            _validationErrors.AddRange(errors);
            AddData(nameof(ValidationErrors), _validationErrors);
        }
    }
}