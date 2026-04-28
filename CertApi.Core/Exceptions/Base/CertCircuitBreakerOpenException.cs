// CertApi.Core/Exceptions/Base/CertCircuitBreakerOpenException.cs
using CertApi.Abstractions.Exceptions;

namespace CertApi.Core.Exceptions.Base;

/// <summary>
/// Exception thrown when a circuit breaker is open and prevents operation execution
/// </summary>
public sealed class CertCircuitBreakerOpenException : CertServiceException
{
    /// <summary>
    /// Gets the circuit name
    /// </summary>
    public string CircuitName { get; }

    /// <summary>
    /// Initializes a new instance of the CertCircuitBreakerOpenException class
    /// </summary>
    public CertCircuitBreakerOpenException()
        : base("Circuit breaker is open", "CircuitBreakerOpen")
    {
        CircuitName = "Default";
    }

    /// <summary>
    /// Initializes a new instance of the CertCircuitBreakerOpenException class with a message
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    public CertCircuitBreakerOpenException(string message)
        : base(message, "CircuitBreakerOpen")
    {
        CircuitName = "Default";
    }

    /// <summary>
    /// Initializes a new instance of the CertCircuitBreakerOpenException class with a message and inner exception
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="inner">Inner exception</param>
    public CertCircuitBreakerOpenException(string message, Exception? inner)
        : base(message, "CircuitBreakerOpen", inner)
    {
        CircuitName = "Default";

        // Preserve correlation ID from inner exception
        if (inner is ICertExceptionInfo certEx && !string.IsNullOrEmpty(certEx.CorrelationId))
        {
            SetCorrelationId(certEx.CorrelationId);
        }
    }

    /// <summary>
    /// Initializes a new instance of the CertCircuitBreakerOpenException class with a message and circuit name
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="circuitName">Circuit name</param>
    public CertCircuitBreakerOpenException(string message, string circuitName)
        : base(message, "CircuitBreakerOpen")
    {
        CircuitName = circuitName ?? "Default";
        AddData(nameof(CircuitName), CircuitName);
    }

    /// <summary>
    /// Initializes a new instance of the CertCircuitBreakerOpenException class with a message, circuit name, and inner exception
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="circuitName">Circuit name</param>
    /// <param name="inner">Inner exception</param>
    public CertCircuitBreakerOpenException(string message, string circuitName, Exception? inner)
        : base(message, "CircuitBreakerOpen", inner)
    {
        CircuitName = circuitName ?? "Default";
        AddData(nameof(CircuitName), CircuitName);

        // Preserve correlation ID from inner exception
        if (inner is ICertExceptionInfo certEx && !string.IsNullOrEmpty(certEx.CorrelationId))
        {
            SetCorrelationId(certEx.CorrelationId);
        }
    }
}
