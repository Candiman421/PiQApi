// PiQApi.Ews.Core/Results/EwsResultFactory.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Results;
using PiQApi.Core.Results;
using PiQApi.Ews.Core.Enums;
using PiQApi.Ews.Core.Results.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace PiQApi.Ews.Core.Results
{
    /// <summary>
    /// Factory for creating EWS operation results
    /// </summary>
    public class EwsResultFactory : IEwsResultFactory
    {
        private readonly IPiQResultFactory _certResultFactory;
        private readonly ILogger<EwsResultFactory> _logger;

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, Exception?> LogSuccessResult =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1, "Success"),
                "Creating successful EWS result with correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> LogSuccessResultWithValue =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(2, "SuccessWithValue"),
                "Creating successful EWS result with value and correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, string, string, Exception?> LogFailureResult =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Debug,
                new EventId(3, "Failure"),
                "Creating failure EWS result with error code: {ErrorCode}, message: {ErrorMessage}, correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, object, string, string, Exception?> LogFailureResultEnum =
            LoggerMessage.Define<object, string, string>(
                LogLevel.Debug,
                new EventId(4, "FailureEnum"),
                "Creating failure EWS result with error code: {ErrorCode}, message: {ErrorMessage}, correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, string, string, Exception?> LogFailureResultException =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Debug,
                new EventId(5, "FailureException"),
                "Creating failure EWS result from exception: {ExceptionType}, message: {ErrorMessage}, correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, string, string, Exception?> LogTypedFailureResult =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Debug,
                new EventId(6, "TypedFailure"),
                "Creating typed failure EWS result with error code: {ErrorCode}, message: {ErrorMessage}, correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, object, string, string, Exception?> LogTypedFailureResultEnum =
            LoggerMessage.Define<object, string, string>(
                LogLevel.Debug,
                new EventId(7, "TypedFailureEnum"),
                "Creating typed failure EWS result with error code: {ErrorCode}, message: {ErrorMessage}, correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, string, string, Exception?> LogTypedFailureResultException =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Debug,
                new EventId(8, "TypedFailureException"),
                "Creating typed failure EWS result from exception: {ExceptionType}, message: {ErrorMessage}, correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> LogCanceledResult =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(9, "Canceled"),
                "Creating canceled EWS result with correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> LogTypedCanceledResult =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(10, "TypedCanceled"),
                "Creating canceled typed EWS result with correlation ID: {CorrelationId}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsResultFactory"/> class
        /// </summary>
        /// <param name="certResultFactory">Core result factory</param>
        /// <param name="logger">Logger</param>
        public EwsResultFactory(IPiQResultFactory certResultFactory, ILogger<EwsResultFactory> logger)
        {
            _certResultFactory = certResultFactory ?? throw new ArgumentNullException(nameof(certResultFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Successful result</returns>
        public IEwsResult Success(string correlationId)
        {
            LogSuccessResult(_logger, correlationId, null);
            return new EwsResult(correlationId);
        }

        /// <summary>
        /// Creates a successful result with a value
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="value">Result value</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Successful result with value</returns>
        public IEwsResult<T> Success<T>(T value, string correlationId)
        {
            LogSuccessResultWithValue(_logger, correlationId, null);
            return new EwsResult<T>(value, correlationId);
        }

        /// <summary>
        /// Creates a failure result
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result</returns>
        public IEwsResult Failure(string errorCode, string errorMessage, string correlationId, OperationStatusType status = OperationStatusType.Failed)
        {
            LogFailureResult(_logger, errorCode, errorMessage, correlationId, null);
            var error = new PiQResultError(errorCode, errorMessage, correlationId);
            return new EwsResult(error, correlationId, status);
        }

        /// <summary>
        /// Creates a failure result with an error code enum
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result</returns>
        public IEwsResult Failure(string errorMessage, ErrorCodeType errorCode, string correlationId, OperationStatusType status = OperationStatusType.Failed)
        {
            LogFailureResultEnum(_logger, errorCode, errorMessage, correlationId, null);
            var coreResult = _certResultFactory.Failure<object>(errorMessage, errorCode, correlationId);
            return new EwsResult(coreResult.ErrorInfo!, correlationId, status);
        }

        /// <summary>
        /// Creates a failure result from an exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result</returns>
        public IEwsResult Failure(Exception exception, string correlationId, OperationStatusType status = OperationStatusType.Failed)
        {
            ArgumentNullException.ThrowIfNull(exception);
            
            LogFailureResultException(_logger, exception.GetType().Name, exception.Message, correlationId, null);
            var coreResult = _certResultFactory.Failure<object>(exception, correlationId);
            return new EwsResult(coreResult.ErrorInfo!, correlationId, status);
        }

        /// <summary>
        /// Creates a failure result with a value type
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="errorCode">Error code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result with value type</returns>
        public IEwsResult<T> Failure<T>(string errorCode, string errorMessage, string correlationId, OperationStatusType status = OperationStatusType.Failed)
        {
            LogTypedFailureResult(_logger, errorCode, errorMessage, correlationId, null);
            var coreResult = _certResultFactory.Failure<T>(errorCode, errorMessage, correlationId);
            return new EwsResult<T>(coreResult.ErrorInfo!, correlationId, status);
        }

        /// <summary>
        /// Creates a failure result with a value type and an error code enum
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result with value type</returns>
        public IEwsResult<T> Failure<T>(string errorMessage, ErrorCodeType errorCode, string correlationId, OperationStatusType status = OperationStatusType.Failed)
        {
            LogTypedFailureResultEnum(_logger, errorCode, errorMessage, correlationId, null);
            var coreResult = _certResultFactory.Failure<T>(errorMessage, errorCode, correlationId);
            return new EwsResult<T>(coreResult.ErrorInfo!, correlationId, status);
        }

        /// <summary>
        /// Creates a failure result with a value type from an exception
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="exception">Exception</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <returns>Failure result with value type</returns>
        public IEwsResult<T> Failure<T>(Exception exception, string correlationId, OperationStatusType status = OperationStatusType.Failed)
        {
            ArgumentNullException.ThrowIfNull(exception);
            
            LogTypedFailureResultException(_logger, exception.GetType().Name, exception.Message, correlationId, null);
            var coreResult = _certResultFactory.Failure<T>(exception, correlationId);
            return new EwsResult<T>(coreResult.ErrorInfo!, correlationId, status);
        }

        /// <summary>
        /// Creates a cancelled result
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Cancelled result</returns>
        public IEwsResult Cancelled(string correlationId)
        {
            LogCanceledResult(_logger, correlationId, null);
            return new EwsResult(correlationId, OperationStatusType.Canceled);
        }

        /// <summary>
        /// Creates a cancelled result with a value type
        /// </summary>
        /// <typeparam name="T">Type of result value</typeparam>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>Cancelled result with value type</returns>
        public IEwsResult<T> Cancelled<T>(string correlationId)
        {
            LogTypedCanceledResult(_logger, correlationId, null);
            
            var cancelError = new PiQResultError(
                "OperationCanceled", 
                "Operation was canceled",
                correlationId);
                
            return new EwsResult<T>(cancelError, correlationId, OperationStatusType.Canceled);
        }
    }
}