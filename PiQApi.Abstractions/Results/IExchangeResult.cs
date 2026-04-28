// PiQApi.Abstractions/Results/IExchangeResult.cs
using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Results
{
    /// <summary>
    /// Defines an Exchange-specific result
    /// </summary>
    public interface IExchangeResult : ICertResult
    {
        ServiceOperationStatusType Status { get; }
        string RequestId { get; }
    }
}