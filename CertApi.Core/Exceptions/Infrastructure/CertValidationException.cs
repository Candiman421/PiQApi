// CertApi.Core/Exceptions/Infrastructure/CertValidationException.cs
using CertApi.Abstractions.Validation.Models;
using CertApi.Core.Exceptions.Base;

namespace CertApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class CertValidationException : CertException
{
    /// <summary>
    /// Gets the type of entity that failed validation
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the collection of validation errors
    /// </summary>
    public IReadOnlyCollection<CertValidationResult> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertValidationException"/> class
    /// </summary>
    public CertValidationException()
        : base("A validation error occurred", "CertValidationError")
    {
        EntityType = string.Empty;
        Errors = new List<CertValidationResult>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertValidationException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public CertValidationException(string message)
        : base(message, "CertValidationError")
    {
        EntityType = string.Empty;
        Errors = new List<CertValidationResult>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertValidationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertValidationException(string message, Exception? inner)
        : base(message, "CertValidationError", inner)
    {
        EntityType = string.Empty;
        Errors = new List<CertValidationResult>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertValidationException"/> class with a specified error message
    /// and entity type
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="entityType">The type of entity that failed validation</param>
    public CertValidationException(string message, string entityType)
        : base(message, "CertValidationError")
    {
        EntityType = entityType ?? string.Empty;
        Errors = new List<CertValidationResult>();

        if (!string.IsNullOrEmpty(entityType))
        {
            AddData(nameof(EntityType), entityType);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertValidationException"/> class with a specified error message,
    /// entity type, optional validation errors, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="entityType">The type of entity that failed validation</param>
    /// <param name="errors">The validation errors that occurred</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public CertValidationException(string message, string entityType, IEnumerable<CertValidationResult>? errors = null, Exception? inner = null)
        : base(message, "CertValidationError", inner)
    {
        EntityType = entityType ?? string.Empty;

        // Materialize the collection immediately to avoid multiple enumeration
        var errorsList = errors?.ToList() ?? new List<CertValidationResult>();
        Errors = errorsList.AsReadOnly();

        if (!string.IsNullOrEmpty(entityType))
        {
            AddData(nameof(EntityType), entityType);
        }

        if (errorsList.Count > 0)
        {
            var errorCount = errorsList.Sum(r => r.Errors.Count);
            AddData("ValidationErrorCount", errorCount);

            var errorMessages = errorsList
                .SelectMany(r => r.Errors)
                .Select(e => e.ToString())
                .ToList();

            if (errorMessages.Count > 0)
            {
                AddData("ValidationErrors", errorMessages);
            }
        }
    }
}