// CertApi.Abstractions/Results/IExchangeResult.cs
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Results
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