// CertApi.Core/Service/CertServiceResponse{T}.cs
using System.Diagnostics.CodeAnalysis;
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Service;
using CertApi.Abstractions.Validation;

namespace CertApi.Core.Service;

/// <summary>
/// Generic implementation of a service response
/// </summary>
/// <typeparam name="T">Type of result</typeparam>
public class CertServiceResponse<T> : CertServiceResponse, ICertServiceResponse<T> where T : class
{
    private readonly T? _result;

    /// <summary>
    /// Gets the result of the service operation
    /// </summary>
    T? ICertServiceResponse<T>.Result => _result;

    /// <summary>
    /// Gets the result as an object (implements base property)
    /// </summary>
    public override object? Result => _result;

    /// <summary>
    /// Creates a new typed service response
    /// </summary>
    public CertServiceResponse(
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
    public static CertServiceResponse<T> FromValidation(
        ICertValidationResult validationResult,
        T? result = null,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        return new CertServiceResponse<T>(
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
    public static CertServiceResponse<T> Success(T? result, string? correlationId = null)
    {
        return new CertServiceResponse<T>(
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
    public static CertServiceResponse<T> FromException(
        Exception? ex,
        T? result = null,
        string? correlationId = null)
    {
        return new CertServiceResponse<T>(
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
    public new CertServiceResponse<T> WithProperty(string key, object value)
    {
        var properties = new Dictionary<string, object>(Properties) { [key] = value };
        return new CertServiceResponse<T>(
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
    public new CertServiceResponse<T> WithProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var mergedProperties = new Dictionary<string, object>(Properties);
        foreach (var property in properties)
        {
            mergedProperties[property.Key] = property.Value;
        }

        return new CertServiceResponse<T>(
            _result,
            Status,
            ErrorCode,
            ErrorMessage,
            CorrelationId,
            Exception,
            mergedProperties);
    }
}