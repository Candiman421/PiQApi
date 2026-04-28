// PiQApi.Core/Exceptions/Infrastructure/CertPropertyValidationException.cs
namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when property validation fails
/// </summary>
public class CertPropertyValidationException : CertValidationException
{
    /// <summary>
    /// Gets the name of the property that failed validation
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertPropertyValidationException"/> class
    /// </summary>
    public CertPropertyValidationException()
        : base("A property validation error occurred", "PropertyValidation")
    {
        PropertyName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertPropertyValidationException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public CertPropertyValidationException(string message)
        : base(message, "PropertyValidation")
    {
        PropertyName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertPropertyValidationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertPropertyValidationException(string message, Exception? inner)
        : base(message, "PropertyValidation", null, inner)
    {
        PropertyName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertPropertyValidationException"/> class with a specified error message
    /// and property name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="propertyName">The name of the property that failed validation</param>
    public CertPropertyValidationException(string message, string propertyName)
        : base(message, "PropertyValidation")
    {
        PropertyName = propertyName ?? string.Empty;
        AddData(nameof(PropertyName), PropertyName);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertPropertyValidationException"/> class with a specified error message,
    /// property name, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="propertyName">The name of the property that failed validation</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertPropertyValidationException(string message, string propertyName, Exception? inner)
        : base(message, "PropertyValidation", null, inner)
    {
        PropertyName = propertyName ?? string.Empty;
        AddData(nameof(PropertyName), PropertyName);
    }
}