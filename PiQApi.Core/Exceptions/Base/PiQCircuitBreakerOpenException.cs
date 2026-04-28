// PiQApi.Core/Exceptions/Base/PiQCircuitBreakerOpenException.cs
using PiQApi.Abstractions.Exceptions;

namespace PiQApi.Core.Exceptions.Base;

/// <summary>
/// Exception thrown when a circuit breaker is open and prevents operation execution
/// </summary>
public sealed class PiQCircuitBreakerOpenException : PiQServiceException
{
    /// <summary>
    /// Gets the circuit name
    /// </summary>
    public string CircuitName { get; }

    /// <summary>
    /// Initializes a new instance of the PiQCircuitBreakerOpenException class
    /// </summary>
    public PiQCircuitBreakerOpenException()
        : base("Circuit breaker is open", "CircuitBreakerOpen")
    {
        CircuitName = "Default";
    }

    /// <summary>
    /// Initializes a new instance of the PiQCircuitBreakerOpenException class with a message
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    public PiQCircuitBreakerOpenException(string message)
        : base(message, "CircuitBreakerOpen")
    {
        CircuitName = "Default";
    }

    /// <summary>
    /// Initializes a new instance of the PiQCircuitBreakerOpenException class with a message and inner exception
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="inner">Inner exception</param>
    public PiQCircuitBreakerOpenException(string message, Exception? inner)
        : base(message, "CircuitBreakerOpen", inner)
    {
        CircuitName = "Default";

        // Preserve correlation ID from inner exception
        if (inner is IPiQExceptionInfo piqEx && !string.IsNullOrEmpty(piqEx.CorrelationId))
        {
            SetCorrelationId(piqEx.CorrelationId);
        }
    }

    /// <summary>
    /// Initializes a new instance of the PiQCircuitBreakerOpenException class with a message and circuit name
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="circuitName">Circuit name</param>
    public PiQCircuitBreakerOpenException(string message, string circuitName)
        : base(message, "CircuitBreakerOpen")
    {
        CircuitName = circuitName ?? "Default";
        AddData(nameof(CircuitName), CircuitName);
    }

    /// <summary>
    /// Initializes a new instance of the PiQCircuitBreakerOpenException class with a message, circuit name, and inner exception
    /// </summary>
    /// <param name="message">ErrorInfo message</param>
    /// <param name="circuitName">Circuit name</param>
    /// <param name="inner">Inner exception</param>
    public PiQCircuitBreakerOpenException(string message, string circuitName, Exception? inner)
        : base(message, "CircuitBreakerOpen", inner)
    {
        CircuitName = circuitName ?? "Default";
        AddData(nameof(CircuitName), CircuitName);

        // Preserve correlation ID from inner exception
        if (inner is IPiQExceptionInfo piqEx && !string.IsNullOrEmpty(piqEx.CorrelationId))
        {
            SetCorrelationId(piqEx.CorrelationId);
        }
    }
}
