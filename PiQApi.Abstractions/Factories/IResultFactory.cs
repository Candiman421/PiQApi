// PiQApi.Abstractions/Factories/IResultFactory.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;

namespace PiQApi.Abstractions.Factories
{
    /// <summary>
    /// Defines factory for creating result objects
    /// </summary>
    public interface IResultFactory
    {
        IPiQResult<T> Success<T>(T value);
        IPiQResult<T> Failure<T>(string code, string message);
        IExchangeResult<T> ExchangeSuccess<T>(T value, ServiceOperationStatusType status, string requestId);
        IExchangeResult<T> ExchangeFailure<T>(string code, string message, ServiceOperationStatusType status, string requestId);
    }
}