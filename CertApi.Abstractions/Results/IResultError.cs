// CertApi.Abstractions/Results/IResultError.cs
using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Results
{
    /// <summary>
    /// Defines the contract for operation errors
    /// </summary>
    public interface IResultError
    {
        ErrorCodeType Code { get; }
        string Message { get; }
        string CorrelationId { get; }
        DateTimeOffset Timestamp { get; }
        ValidationSeverityType Severity { get; }
        IReadOnlyDictionary<string, object> Context { get; }
    }
}