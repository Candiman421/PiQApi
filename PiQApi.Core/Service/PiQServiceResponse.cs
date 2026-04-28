// PiQApi.Core/Service/PiQServiceResponse.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Service;

namespace PiQApi.Core.Service;

/// <summary>
/// Base implementation of a service response
/// </summary>
public class PiQServiceResponse : IPiQServiceResponse
{
    /// <summary>
    /// Gets the status of the service operation
    /// </summary>
    public ResultStatusType Status { get; }

    /// <summary>
    /// Gets the error code if operation was not successful
    /// </summary>
    public ErrorCodeType ErrorCode { get; }

    /// <summary>
    /// Gets the error message if operation was not successful
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the timestamp when the response was created
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the correlation ID for tracking the operation
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets whether the operation was successful
    /// </summary>
    public bool IsSuccess => Status == ResultStatusType.Success;

    /// <summary>
    /// Gets whether the operation was successful (alias for IsSuccess)
    /// </summary>
    public bool IsSuccessful => IsSuccess;

    /// <summary>
    /// Gets the exception that caused the error, if any
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the result object associated with this response
    /// </summary>
    public virtual object? Result { get; init; }

    /// <summary>
    /// Gets additional properties associated with this response
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Creates a new service response
    /// </summary>
    public PiQServiceResponse(
        ResultStatusType status,
        ErrorCodeType errorCode,
        string errorMessage,
        string? correlationId = null,
        Exception? exception = null,
        IReadOnlyDictionary<string, object>? properties = null)
    {
        Status = status;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage ?? string.Empty;
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        Timestamp = DateTimeOffset.UtcNow;
        Exception = exception;
        Properties = properties ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a successful service response
    /// </summary>
    public static PiQServiceResponse Success(string? correlationId = null)
    {
        return new PiQServiceResponse(
            ResultStatusType.Success,
            ErrorCodeType.None,
            string.Empty,
            correlationId);
    }

    /// <summary>
    /// Creates a failed service response from an exception
    /// </summary>
    public static PiQServiceResponse FromException(
        Exception? ex,
        string? correlationId = null)
    {
        return new PiQServiceResponse(
            ResultStatusType.Failed,
            ErrorCodeType.InternalServerError,
            ex?.Message ?? "An error occurred",
            correlationId,
            ex);
    }

    /// <summary>
    /// Creates a new response with additional property
    /// </summary>
    public PiQServiceResponse WithProperty(string key, object value)
    {
        var properties = new Dictionary<string, object>(Properties) { [key] = value };
        return new PiQServiceResponse(
            Status,
            ErrorCode,
            ErrorMessage,
            CorrelationId,
            Exception,
            properties);
    }

    /// <summary>
    /// Creates a new response with additional properties
    /// </summary>
    public PiQServiceResponse WithProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var mergedProperties = new Dictionary<string, object>(Properties);
        foreach (var property in properties)
        {
            mergedProperties[property.Key] = property.Value;
        }

        return new PiQServiceResponse(
            Status,
            ErrorCode,
            ErrorMessage,
            CorrelationId,
            Exception,
            mergedProperties);
    }
}