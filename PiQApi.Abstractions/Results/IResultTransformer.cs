// PiQApi.Abstractions/Results/IResultTransformer.cs
using PiQApi.Abstractions.Validation;

namespace PiQApi.Abstractions.Results
{
    /// <summary>
    /// Transforms between different result types
    /// </summary>
    public interface IResultTransformer
    {
        IPiQResult<T> FromValidation<T>(PiQValidationResult validation, T? value = default);
        IExchangeResult<T> FromValidationToExchange<T>(PiQValidationResult validation, T? value = default, string requestId = "");
        IPiQResult<TNew> Map<T, TNew>(IPiQResult<T> result, Func<T, TNew> mapper);
        IExchangeResult<TNew> MapExchange<T, TNew>(IExchangeResult<T> result, Func<T, TNew> mapper);
    }
}