// CertApi.Core/Results/CertResultTransformer.cs
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Results;
using CertApi.Abstractions.Validation;

namespace CertApi.Core.Results;

/// <summary>
/// Implementation of the result transformer for converting between result types
/// </summary>
public class CertResultTransformer : ICertResultTransformer
{
    private readonly ICertCorrelationContext _correlationContext;
    private readonly ICertResultFactory _resultFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertResultTransformer"/> class
    /// </summary>
    /// <param name="correlationContext">The correlation context for tracking operations</param>
    /// <param name="resultFactory">The result factory for creating result objects</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public CertResultTransformer(
        ICertCorrelationContext correlationContext,
        ICertResultFactory resultFactory)
    {
        _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
    }

    /// <summary>
    /// Creates a result from a validation result
    /// </summary>
    /// <typeparam name="T">Type of the result value</typeparam>
    /// <param name="validation">Validation result</param>
    /// <param name="value">Optional value to include in successful results</param>
    /// <returns>A result object representing the validation outcome</returns>
    /// <exception cref="ArgumentNullException">Thrown when validation is null</exception>
    public ICertResult<T> FromValidation<T>(ICertValidationResult validation, T? value = default)
    {
        ArgumentNullException.ThrowIfNull(validation);

        // Track operation in correlation context
        _correlationContext.AddProperty("TransformOperation", "ValidationToResult");
        _correlationContext.AddProperty("ValidationIsValid", validation.IsValid);

        // Get correlation ID from validation context first, then fall back to ambient context
        string correlationId = validation.GetCorrelationId() ?? _correlationContext.CorrelationId;

        if (validation.IsValid)
        {
            var result = _resultFactory.Success(value!, correlationId);

            // Copy validation context to result properties
            if (validation.Context.Count > 0)
            {
                var props = new Dictionary<string, object>();
                foreach (var prop in validation.Context)
                {
                    if (prop.Key != "CorrelationId") // Avoid duplicating correlation ID
                    {
                        props[prop.Key] = prop.Value;
                    }
                }

                if (props.Count > 0)
                {
                    return result.WithProperties(props);
                }
            }

            return result;
        }
        else
        {
            // Extract validation error information
            _correlationContext.AddProperty("ValidationErrorCount", validation.Errors.Count);

            // Convert errors to a list for easier and safer access
            var errors = validation.Errors.ToList();

            var errorMessage = errors.Count > 0
                ? errors[0].Message
                : "Validation failed";

            var errorCode = errors.Count > 0
                ? errors[0].Code
                : "VALIDATION_ERROR";

            var result = _resultFactory.Failure<T>(errorCode, errorMessage, correlationId);

            // Add validation properties and errors to result properties
            var props = new Dictionary<string, object>();

            // Copy validation context (excluding correlation ID which is already set)
            foreach (var prop in validation.Context)
            {
                if (prop.Key != "CorrelationId")
                {
                    props[$"Validation_{prop.Key}"] = prop.Value;
                }
            }

            // Add error details
            if (errors.Count > 0)
            {
                props["ValidationErrors"] = errors.Count;

                for (int i = 0; i < errors.Count && i < 5; i++)
                {
                    props[$"Error_{i}_Code"] = errors[i].Code;
                    props[$"Error_{i}_Message"] = errors[i].Message;

                    if (!string.IsNullOrEmpty(errors[i].PropertyName))
                    {
                        props[$"Error_{i}_Property"] = errors[i].PropertyName;
                    }
                }
            }

            if (props.Count > 0)
            {
                return result.WithProperties(props);
            }

            return result;
        }
    }

    /// <summary>
    /// Creates a service result from a validation result
    /// </summary>
    /// <typeparam name="T">Type of the result value</typeparam>
    /// <param name="validation">Validation result</param>
    /// <param name="value">Optional value to include in successful results</param>
    /// <param name="requestId">Request ID for the service operation</param>
    /// <returns>A service result object representing the validation outcome</returns>
    /// <exception cref="ArgumentNullException">Thrown when validation is null</exception>
    public ICertServiceResult<T> FromValidationToService<T>(
        ICertValidationResult validation,
        T? value = default,
        string requestId = "")
    {
        ArgumentNullException.ThrowIfNull(validation);

        // Track operation in correlation context
        _correlationContext.AddProperty("TransformOperation", "ValidationToServiceResult");
        _correlationContext.AddProperty("ValidationIsValid", validation.IsValid);
        _correlationContext.AddProperty("HasRequestId", !string.IsNullOrEmpty(requestId));

        // Get correlation ID from validation context first, then fall back to ambient context
        string correlationId = validation.GetCorrelationId() ?? _correlationContext.CorrelationId;

        var effectiveRequestId = !string.IsNullOrEmpty(requestId)
            ? requestId
            : Guid.NewGuid().ToString();

        if (validation.IsValid)
        {
            var result = _resultFactory.ServiceSuccess(
                value!,
                Abstractions.Enums.OperationStatusType.Done,
                effectiveRequestId,
                correlationId);

            // Copy validation context to result properties
            if (validation.Context.Count > 0)
            {
                var props = new Dictionary<string, object>();
                foreach (var prop in validation.Context)
                {
                    if (prop.Key != "CorrelationId") // Avoid duplicating correlation ID
                    {
                        props[prop.Key] = prop.Value;
                    }
                }

                if (props.Count > 0)
                {
                    return result.WithProperties(props);
                }
            }

            return result;
        }
        else
        {
            // Extract validation error information
            _correlationContext.AddProperty("ValidationErrorCount", validation.Errors.Count);

            // Convert errors to a list for easier and safer access
            var errors = validation.Errors.ToList();

            var errorMessage = errors.Count > 0
                ? errors[0].Message
                : "Validation failed";

            var errorCode = errors.Count > 0
                ? errors[0].Code
                : "VALIDATION_ERROR";

            var result = _resultFactory.ServiceFailure<T>(
                errorCode,
                errorMessage,
                Abstractions.Enums.OperationStatusType.Failed,
                effectiveRequestId,
                correlationId);

            // Add validation properties and errors to result properties
            var props = new Dictionary<string, object>();

            // Copy validation context (excluding correlation ID which is already set)
            foreach (var prop in validation.Context)
            {
                if (prop.Key != "CorrelationId")
                {
                    props[$"Validation_{prop.Key}"] = prop.Value;
                }
            }

            // Add error details
            if (errors.Count > 0)
            {
                props["ValidationErrors"] = errors.Count;

                for (int i = 0; i < errors.Count && i < 5; i++)
                {
                    props[$"Error_{i}_Code"] = errors[i].Code;
                    props[$"Error_{i}_Message"] = errors[i].Message;

                    if (!string.IsNullOrEmpty(errors[i].PropertyName))
                    {
                        props[$"Error_{i}_Property"] = errors[i].PropertyName;
                    }
                }
            }

            if (props.Count > 0)
            {
                return result.WithProperties(props);
            }

            return result;
        }
    }

    /// <summary>
    /// Maps a result value to a different type
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <typeparam name="TNew">Destination type</typeparam>
    /// <param name="result">Source result</param>
    /// <param name="mapper">Function to map from source to destination type</param>
    /// <returns>New result with mapped value</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or mapper is null</exception>
    public ICertResult<TNew> Map<T, TNew>(ICertResult<T> result, Func<T, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(mapper);

        // Track operation in correlation context
        _correlationContext.AddProperty("TransformOperation", "MapResult");
        _correlationContext.AddProperty("SourceType", typeof(T).Name);
        _correlationContext.AddProperty("DestinationType", typeof(TNew).Name);
        _correlationContext.AddProperty("ResultIsSuccess", result.IsSuccess);

        if (result.IsSuccess)
        {
            // Map the value for successful results
            if (result.Value != null)
            {
                try
                {
                    var mappedValue = mapper(result.Value);
                    var newResult = _resultFactory.Success(mappedValue, result.CorrelationId);

                    // Copy properties from original result
                    if (result.Properties.Count > 0)
                    {
                        return newResult.WithProperties(result.Properties.ToDictionary(p => p.Key, p => p.Value));
                    }

                    return newResult;
                }
                catch (Exception ex)
                {
                    // Record mapping error in correlation context
                    _correlationContext.AddProperty("MappingError", ex.Message);
                    _correlationContext.AddProperty("MappingErrorType", ex.GetType().Name);

                    // Create failure result with mapping error
                    return _resultFactory.Failure<TNew>(
                        "MAPPING_ERROR",
                        $"Error mapping from {typeof(T).Name} to {typeof(TNew).Name}: {ex.Message}",
                        result.CorrelationId);
                }
            }
            else
            {
                // Cannot map null values
                _correlationContext.AddProperty("MappingError", "Source value is null");
                return _resultFactory.Failure<TNew>(
                    "MAPPING_ERROR",
                    $"Cannot map null value from {typeof(T).Name} to {typeof(TNew).Name}",
                    result.CorrelationId);
            }
        }
        else
        {
            // For failure results, propagate the error information
            var newResult = _resultFactory.Failure<TNew>(
                result.ErrorInfo?.Code ?? "UNKNOWN_ERROR",
                result.ErrorInfo?.Message ?? "An unknown error occurred",
                result.CorrelationId);

            // Copy properties from original result
            if (result.Properties.Count > 0)
            {
                return newResult.WithProperties(result.Properties.ToDictionary(p => p.Key, p => p.Value));
            }

            return newResult;
        }
    }

    /// <summary>
    /// Maps a service result value to a different type
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <typeparam name="TNew">Destination type</typeparam>
    /// <param name="result">Source service result</param>
    /// <param name="mapper">Function to map from source to destination type</param>
    /// <returns>New service result with mapped value</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or mapper is null</exception>
    public ICertServiceResult<TNew> MapService<T, TNew>(
        ICertServiceResult<T> result,
        Func<T, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(mapper);

        // Track operation in correlation context
        _correlationContext.AddProperty("TransformOperation", "MapServiceResult");
        _correlationContext.AddProperty("SourceType", typeof(T).Name);
        _correlationContext.AddProperty("DestinationType", typeof(TNew).Name);
        _correlationContext.AddProperty("ResultIsSuccess", result.IsSuccess);
        _correlationContext.AddProperty("ResultStatus", result.Status.ToString());

        if (result.IsSuccess)
        {
            // Map the value for successful results
            if (result.Value != null)
            {
                try
                {
                    var mappedValue = mapper(result.Value);
                    var newResult = _resultFactory.ServiceSuccess(
                        mappedValue,
                        result.Status,
                        result.RequestId,
                        result.CorrelationId);

                    // Copy properties from original result
                    if (result.Properties.Count > 0)
                    {
                        return newResult.WithProperties(result.Properties.ToDictionary(p => p.Key, p => p.Value));
                    }

                    return newResult;
                }
                catch (Exception ex)
                {
                    // Record mapping error in correlation context
                    _correlationContext.AddProperty("MappingError", ex.Message);
                    _correlationContext.AddProperty("MappingErrorType", ex.GetType().Name);

                    // Create failure result with mapping error
                    return _resultFactory.ServiceFailure<TNew>(
                        "MAPPING_ERROR",
                        $"Error mapping from {typeof(T).Name} to {typeof(TNew).Name}: {ex.Message}",
                        Abstractions.Enums.OperationStatusType.Failed,
                        result.RequestId,
                        result.CorrelationId);
                }
            }
            else
            {
                // Cannot map null values
                _correlationContext.AddProperty("MappingError", "Source value is null");
                return _resultFactory.ServiceFailure<TNew>(
                    "MAPPING_ERROR",
                    $"Cannot map null value from {typeof(T).Name} to {typeof(TNew).Name}",
                    Abstractions.Enums.OperationStatusType.Failed,
                    result.RequestId,
                    result.CorrelationId);
            }
        }
        else
        {
            // For failure results, propagate the error information
            var newResult = _resultFactory.ServiceFailure<TNew>(
                result.ErrorInfo?.Code ?? "UNKNOWN_ERROR",
                result.ErrorInfo?.Message ?? "An unknown error occurred",
                result.Status,
                result.RequestId,
                result.CorrelationId);

            // Copy properties from original result
            if (result.Properties.Count > 0)
            {
                return newResult.WithProperties(result.Properties.ToDictionary(p => p.Key, p => p.Value));
            }

            return newResult;
        }
    }
}