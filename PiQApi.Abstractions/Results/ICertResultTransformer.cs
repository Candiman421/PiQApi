// PiQApi.Abstractions/Results/ICertResultTransformer.cs
using PiQApi.Abstractions.Validation;

namespace PiQApi.Abstractions.Results;

/// <summary>
/// Transforms between different result types
/// </summary>
public interface ICertResultTransformer
{
    /// <summary>
    /// Creates a result from a validation result
    /// </summary>
    ICertResult<T> FromValidation<T>(ICertValidationResult validation, T? value = default);

    /// <summary>
    /// Creates a service result from a validation result
    /// </summary>
    ICertServiceResult<T> FromValidationToService<T>(ICertValidationResult validation, T? value = default, string requestId = "");

    /// <summary>
    /// Maps a result value to a different type
    /// </summary>
    ICertResult<TNew> Map<T, TNew>(ICertResult<T> result, Func<T, TNew> mapper);

    /// <summary>
    /// Maps a service result value to a different type
    /// </summary>
    ICertServiceResult<TNew> MapService<T, TNew>(ICertServiceResult<T> result, Func<T, TNew> mapper);
}
