// CertApi.Abstractions/Results/IExchangeResult{T}.cs
// CertApi.Abstractions/Results/IExchangeResult{T}.cs
// CertApi.Abstractions/Results/IExchangeResult{T}.cs
// CertApi.Abstractions/Results/IExchangeResult{T}.cs
namespace CertApi.Abstractions.Results
{
    /// <summary>
    /// Defines an Exchange-specific result containing a value
    /// </summary>
    public interface IExchangeResult<out T> : IExchangeResult, ICertResult<T>
    {
    }
}