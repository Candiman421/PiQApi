// CertApi.Core/Exceptions/Infrastructure/CertBuilderException.cs
using CertApi.Core.Exceptions.Base;

namespace CertApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when a builder pattern operation fails
/// </summary>
public sealed class CertBuilderException : CertServiceException
{
    /// <summary>
    /// Gets the name of the property that caused the error
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the value that was attempted to be set
    /// </summary>
    public object? AttemptedValue { get; }

    /// <summary>
    /// Gets the current state of the builder
    /// </summary>
    public string? CurrentState { get; }

    /// <summary>
    /// Gets the required state for the operation
    /// </summary>
    public string? RequiredState { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertBuilderException"/> class
    /// </summary>
    public CertBuilderException()
        : base("A builder error occurred", "BuilderError")
    {
        PropertyName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertBuilderException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public CertBuilderException(string message)
        : base(message, "BuilderError")
    {
        PropertyName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertBuilderException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertBuilderException(string message, Exception? inner)
        : base(message, "BuilderError", inner)
    {
        PropertyName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertBuilderException"/> class with a specified error message
    /// and property name
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="propertyName">The name of the property that caused the error</param>
    public CertBuilderException(string message, string propertyName)
        : base(message, "BuilderError")
    {
        PropertyName = propertyName ?? string.Empty;

        if (!string.IsNullOrEmpty(propertyName))
            AddData(nameof(PropertyName), propertyName);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertBuilderException"/> class with a specified error message,
    /// property name, and error code
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="propertyName">The name of the property that caused the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    public CertBuilderException(string message, string propertyName, string? errorCode)
        : base(message, errorCode ?? "BuilderError")
    {
        PropertyName = propertyName ?? string.Empty;

        if (!string.IsNullOrEmpty(propertyName))
            AddData(nameof(PropertyName), propertyName);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertBuilderException"/> class with a specified error message,
    /// property name, and attempted value
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="propertyName">The name of the property that caused the error</param>
    /// <param name="attemptedValue">The value that was attempted to be set</param>
    public CertBuilderException(string message, string propertyName, object? attemptedValue)
        : base(message, "BuilderError")
    {
        PropertyName = propertyName ?? string.Empty;
        AttemptedValue = attemptedValue;

        if (!string.IsNullOrEmpty(propertyName))
            AddData(nameof(PropertyName), propertyName);

        if (attemptedValue != null)
            AddData(nameof(AttemptedValue), attemptedValue);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertBuilderException"/> class with a specified error message,
    /// property name, error code, attempted value, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="propertyName">The name of the property that caused the error</param>
    /// <param name="errorCode">The error code associated with this exception</param>
    /// <param name="attemptedValue">The value that was attempted to be set</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertBuilderException(string message, string propertyName, string? errorCode, object? attemptedValue, Exception? inner)
        : base(message, errorCode ?? "BuilderError", inner)
    {
        PropertyName = propertyName ?? string.Empty;
        AttemptedValue = attemptedValue;

        if (!string.IsNullOrEmpty(propertyName))
            AddData(nameof(PropertyName), propertyName);

        if (attemptedValue != null)
            AddData(nameof(AttemptedValue), attemptedValue);
    }

    /// <summary>
    /// Creates an exception for an invalid builder state
    /// </summary>
    /// <param name="currentState">The current state of the builder</param>
    /// <param name="requiredState">The required state for the operation</param>
    /// <param name="inner">The inner exception</param>
    /// <returns>A <see cref="CertBuilderException"/> for invalid builder state</returns>
    public static CertBuilderException ForInvalidState(string currentState, string requiredState, Exception? inner = null)
    {
        var message = $"Builder is in state '{currentState}' but requires state '{requiredState}' for this operation";
        var exception = new CertBuilderException(message, "BuilderStateError", inner);

        exception.AddData("CurrentState", currentState);
        exception.AddData("RequiredState", requiredState);

        return exception;
    }

    /// <summary>
    /// Returns a string that represents the current object
    /// </summary>
    /// <returns>A string that represents the current object</returns>
    public override string ToString()
    {
        var baseString = base.ToString();
        var details = new List<string>();

        if (!string.IsNullOrEmpty(PropertyName))
            details.Add($"Property: {PropertyName}");

        if (AttemptedValue != null)
            details.Add($"Attempted Value: {AttemptedValue}");

        if (!string.IsNullOrEmpty(CurrentState))
            details.Add($"Current State: {CurrentState}");

        if (!string.IsNullOrEmpty(RequiredState))
            details.Add($"Required State: {RequiredState}");

        return details.Count > 0
            ? $"{baseString}{Environment.NewLine}Additional Details: {string.Join(", ", details)}"
            : baseString;
    }
}