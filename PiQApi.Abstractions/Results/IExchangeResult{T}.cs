// PiQApi.Abstractions/Results/IExchangeResult{T}.cs
// PiQApi.Abstractions/Results/IExchangeResult{T}.cs
// PiQApi.Abstractions/Results/IExchangeResult{T}.cs
// PiQApi.Abstractions/Results/IExchangeResult{T}.cs
namespace PiQApi.Abstractions.Results
{
    /// <summary>
    /// Defines an Exchange-specific result containing a value
    /// </summary>
    public interface IExchangeResult<out T> : IExchangeResult, IPiQResult<T>
    {
    }
}