// PiQApi.Core/Service/PiQServiceResponse{T}.cs
using System.Diagnostics.CodeAnalysis;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Service;
using PiQApi.Abstractions.Validation;

namespace PiQApi.Core.Service;

/// <summary>
/// Generic implementation of a service response
/// </summary>
/// <typeparam name="T">Type of result</typeparam>
public class PiQServiceResponse<T> : PiQServiceResponse, IPiQServiceResponse<T> where T : class
{
    private readonly T? _result;

    /// <summary>
    /// Gets the result of the service operation
    /// </summary>
    T? IPiQServiceResponse<T>.Result => _result;

    /// <summary>
    /// Gets the result as an object (implements base property)
    /// </summary>
    public override object? Result => _result;

    /// <summary>
    /// Creates a new typed service response
    /// </summary>
    public PiQServiceResponse(
        T? result,
        ResultStatusType status,
        ErrorCodeType errorCode,
        string errorMessage,
        string? correlationId = null,
        Exception? exception = null,
        IReadOnlyDictionary<string, object>? properties = null)
        : base(status, errorCode, errorMessage, correlationId, exception, properties)
    {
        _result = result;
    }

    /// <summary>
    /// Creates a service response from a validation result
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
        Justification = "Intentional factory method pattern for typed responses")]
    public static PiQServiceResponse<T> FromValidation(
        IPiQValidationResult validationResult,
        T? result = null,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        return new PiQServiceResponse<T>(
            result,
            validationResult.IsValid ? ResultStatusType.Success : ResultStatusType.Failed,
            validationResult.IsValid ? ErrorCodeType.None : ErrorCodeType.ValidationError,
            validationResult.IsValid ? string.Empty : validationResult.GetErrorSummary(),
            correlationId);
    }

    /// <summary>
    /// Creates a successful typed service response
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
        Justification = "Intentional factory method pattern for typed responses")]
    public static PiQServiceResponse<T> Success(T? result, string? correlationId = null)
    {
        return new PiQServiceResponse<T>(
            result,
            ResultStatusType.Success,
            ErrorCodeType.None,
            string.Empty,
            correlationId);
    }

    /// <summary>
    /// Creates a failed service response from an exception
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
        Justification = "Intentional factory method pattern for typed responses")]
    public static PiQServiceResponse<T> FromException(
        Exception? ex,
        T? result = null,
        string? correlationId = null)
    {
        return new PiQServiceResponse<T>(
            result,
            ResultStatusType.Failed,
            ErrorCodeType.InternalServerError,
            ex?.Message ?? "An error occurred",
            correlationId,
            ex);
    }

    /// <summary>
    /// Creates a new response with additional property
    /// </summary>
    public new PiQServiceResponse<T> WithProperty(string key, object value)
    {
        var properties = new Dictionary<string, object>(Properties) { [key] = value };
        return new PiQServiceResponse<T>(
            _result,
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
    public new PiQServiceResponse<T> WithProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var mergedProperties = new Dictionary<string, object>(Properties);
        foreach (var property in properties)
        {
            mergedProperties[property.Key] = property.Value;
        }

        return new PiQServiceResponse<T>(
            _result,
            Status,
            ErrorCode,
            ErrorMessage,
            CorrelationId,
            Exception,
            mergedProperties);
    }
}