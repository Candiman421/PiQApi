// PiQApi.Abstractions/Results/IPiQResultTransformer.cs
using PiQApi.Abstractions.Validation;

namespace PiQApi.Abstractions.Results;

/// <summary>
/// Transforms between different result types
/// </summary>
public interface IPiQResultTransformer
{
    /// <summary>
    /// Creates a result from a validation result
    /// </summary>
    IPiQResult<T> FromValidation<T>(IPiQValidationResult validation, T? value = default);

    /// <summary>
    /// Creates a service result from a validation result
    /// </summary>
    IPiQServiceResult<T> FromValidationToService<T>(IPiQValidationResult validation, T? value = default, string requestId = "");

    /// <summary>
    /// Maps a result value to a different type
    /// </summary>
    IPiQResult<TNew> Map<T, TNew>(IPiQResult<T> result, Func<T, TNew> mapper);

    /// <summary>
    /// Maps a service result value to a different type
    /// </summary>
    IPiQServiceResult<TNew> MapService<T, TNew>(IPiQServiceResult<T> result, Func<T, TNew> mapper);
}
