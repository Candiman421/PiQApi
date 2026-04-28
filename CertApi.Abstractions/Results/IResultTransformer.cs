// CertApi.Abstractions/Results/IResultTransformer.cs
using CertApi.Abstractions.Validation;

namespace CertApi.Abstractions.Results
{
    /// <summary>
    /// Transforms between different result types
    /// </summary>
    public interface IResultTransformer
    {
        ICertResult<T> FromValidation<T>(CertValidationResult validation, T? value = default);
        IExchangeResult<T> FromValidationToExchange<T>(CertValidationResult validation, T? value = default, string requestId = "");
        ICertResult<TNew> Map<T, TNew>(ICertResult<T> result, Func<T, TNew> mapper);
        IExchangeResult<TNew> MapExchange<T, TNew>(IExchangeResult<T> result, Func<T, TNew> mapper);
    }
}