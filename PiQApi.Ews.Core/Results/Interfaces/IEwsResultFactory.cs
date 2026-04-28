// PiQApi.Ews.Core/Results/Interfaces/IEwsResultFactory.cs

using PiQApi.Abstractions.Enums;
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Ews.Core.Results.Interfaces
{
    /// <summary>
    /// Factory for creating EWS operation results
    /// </summary>
    public interface IEwsResultFactory
    {
        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Successful result</returns>
        IEwsResult Success(string correlationId);

        /// <summary>
        /// Creates a successful result with a value
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="value">Result value</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Successful result with value</returns>
        IEwsResult<T> Success<T>(T value, string correlationId);

        /// <summary>
        /// Creates a failure result
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result</returns>
        IEwsResult Failure(
            string errorCode,
            string errorMessage,
            string correlationId,
            OperationStatusType status = OperationStatusType.Failed);

        /// <summary>
        /// Creates a failure result with an error code enum
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result</returns>
        IEwsResult Failure(
            string errorMessage,
            ErrorCodeType errorCode,
            string correlationId,
            OperationStatusType status = OperationStatusType.Failed);

        /// <summary>
        /// Creates a failure result from an exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result</returns>
        IEwsResult Failure(
            Exception exception,
            string correlationId,
            OperationStatusType status = OperationStatusType.Failed);

        /// <summary>
        /// Creates a failure result with a value type
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result with value type</returns>
        IEwsResult<T> Failure<T>(
            string errorCode,
            string errorMessage,
            string correlationId,
            OperationStatusType status = OperationStatusType.Failed);

        /// <summary>
        /// Creates a failure result with a value type and an error code enum
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result with value type</returns>
        IEwsResult<T> Failure<T>(
            string errorMessage,
            ErrorCodeType errorCode,
            string correlationId,
            OperationStatusType status = OperationStatusType.Failed);

        /// <summary>
        /// Creates a failure result with a value type from an exception
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="exception">Exception</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result with value type</returns>
        IEwsResult<T> Failure<T>(
            Exception exception,
            string correlationId,
            OperationStatusType status = OperationStatusType.Failed);

        /// <summary>
        /// Creates a cancelled result
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Cancelled result</returns>
        IEwsResult Cancelled(string correlationId);

        /// <summary>
        /// Creates a cancelled result with a value type
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Cancelled result with value type</returns>
        IEwsResult<T> Cancelled<T>(string correlationId);
    }
}