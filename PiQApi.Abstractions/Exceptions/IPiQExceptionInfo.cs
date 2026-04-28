// PiQApi.Abstractions/Exceptions/IPiQExceptionInfo.cs
namespace PiQApi.Abstractions.Exceptions;

/// <summary>
/// Defines contract for all service exceptions
/// </summary>
public interface IPiQExceptionInfo
{
    /// <summary>
    /// Gets the error code for this exception
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    /// Gets the correlation identifier for tracing
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets additional contextual data
    /// </summary>
    IReadOnlyDictionary<string, object> AdditionalData { get; }

    /// <summary>
    /// Gets the timestamp when the exception occurred
    /// </summary>
    DateTimeOffset Timestamp { get; }
}
