// PiQApi.Ews.Core/Results/EwsResult{T}.cs

using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;
using PiQApi.Core.Results;
using PiQApi.Ews.Core.Enums;
using PiQApi.Ews.Core.Results.Interfaces;
using System;
using System.Collections.Generic;

namespace PiQApi.Ews.Core.Results
{
    /// <summary>
    /// Implementation of EWS operation result with a value
    /// </summary>
    /// <typeparam name="T">Type of result value</typeparam>
    public class EwsResult<T> : PiQResult<T>, IEwsResult<T>
    {
        /// <summary>
        /// Gets the operation status
        /// </summary>
        public OperationStatusType Status { get; }

        /// <summary>
        /// Gets the request ID if available
        /// </summary>
        public string? RequestId { get; }

        /// <summary>
        /// Gets the EWS response code if available
        /// </summary>
        public EwsResponseCodeType? EwsResponseCode { get; }

        /// <summary>
        /// Gets the EWS-specific request ID if available (different from the general RequestId)
        /// </summary>
        public string? EwsRequestId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsResult{T}"/> class for a successful result
        /// </summary>
        /// <param name="value">Result value</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="ewsResponseCode">Optional EWS response code</param>
        /// <param name="ewsRequestId">Optional EWS request ID</param>
        public EwsResult(
            T value, 
            string correlationId, 
            OperationStatusType status = OperationStatusType.Done, 
            string? requestId = null,
            EwsResponseCodeType? ewsResponseCode = null,
            string? ewsRequestId = null)
            : base(true, value, null, correlationId, DateTimeOffset.UtcNow, null)
        {
            Status = status;
            RequestId = requestId;
            EwsResponseCode = ewsResponseCode;
            EwsRequestId = ewsRequestId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsResult{T}"/> class for a failed result
        /// </summary>
        /// <param name="error">Error information</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="ewsResponseCode">Optional EWS response code</param>
        /// <param name="ewsRequestId">Optional EWS request ID</param>
        public EwsResult(
            IPiQResultError error, 
            string correlationId, 
            OperationStatusType status = OperationStatusType.Failed, 
            string? requestId = null,
            EwsResponseCodeType? ewsResponseCode = null,
            string? ewsRequestId = null)
            : base(false, default, error, correlationId, DateTimeOffset.UtcNow, null)
        {
            Status = status;
            RequestId = requestId;
            EwsResponseCode = ewsResponseCode;
            EwsRequestId = ewsRequestId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsResult{T}"/> class with custom properties
        /// </summary>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="value">Result value (default for failures)</param>
        /// <param name="error">Error information (null for success)</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="ewsResponseCode">Optional EWS response code</param>
        /// <param name="ewsRequestId">Optional EWS request ID</param>
        /// <param name="properties">Additional properties</param>
        protected EwsResult(
            bool isSuccess,
            T? value,
            IPiQResultError? error,
            string correlationId,
            OperationStatusType status,
            string? requestId,
            EwsResponseCodeType? ewsResponseCode,
            string? ewsRequestId,
            IDictionary<string, object>? properties)
            : base(isSuccess, value, error, correlationId, DateTimeOffset.UtcNow, properties)
        {
            Status = status;
            RequestId = requestId;
            EwsResponseCode = ewsResponseCode;
            EwsRequestId = ewsRequestId;
        }

        /// <summary>
        /// Creates a new result with additional property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        /// <returns>New result with added property</returns>
        public new IEwsResult<T> WithProperty(string key, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            var newProperties = new Dictionary<string, object>(Properties);
            newProperties[key] = value;

            return new EwsResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                CorrelationId,
                Status,
                RequestId,
                EwsResponseCode,
                EwsRequestId,
                newProperties);
        }

        /// <summary>
        /// Creates a new result with additional properties
        /// </summary>
        /// <param name="properties">Properties to add</param>
        /// <returns>New result with added properties</returns>
        public new IEwsResult<T> WithProperties(IDictionary<string, object> properties)
        {
            ArgumentNullException.ThrowIfNull(properties);

            var newProperties = new Dictionary<string, object>(Properties);
            foreach (var kvp in properties)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
                {
                    newProperties[kvp.Key] = kvp.Value;
                }
            }

            return new EwsResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                CorrelationId,
                Status,
                RequestId,
                EwsResponseCode,
                EwsRequestId,
                newProperties);
        }

        /// <summary>
        /// Creates a new result with different status
        /// </summary>
        /// <param name="status">New status</param>
        /// <returns>New result with updated status</returns>
        public IEwsResult<T> WithStatus(OperationStatusType status)
        {
            return new EwsResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                CorrelationId,
                status,
                RequestId,
                EwsResponseCode,
                EwsRequestId,
                new Dictionary<string, object>(Properties));
        }

        /// <summary>
        /// Creates a new result with different request ID
        /// </summary>
        /// <param name="requestId">New request ID</param>
        /// <returns>New result with updated request ID</returns>
        public IEwsResult<T> WithRequestId(string requestId)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            return new EwsResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                CorrelationId,
                Status,
                requestId,
                EwsResponseCode,
                EwsRequestId,
                new Dictionary<string, object>(Properties));
        }

        /// <summary>
        /// Creates a new result with different EWS response code
        /// </summary>
        /// <param name="responseCode">New EWS response code</param>
        /// <returns>New result with updated EWS response code</returns>
        public IEwsResult<T> WithEwsResponseCode(EwsResponseCodeType responseCode)
        {
            return new EwsResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                CorrelationId,
                Status,
                RequestId,
                responseCode,
                EwsRequestId,
                new Dictionary<string, object>(Properties));
        }

        /// <summary>
        /// Creates a new result with different EWS request ID
        /// </summary>
        /// <param name="requestId">New EWS request ID</param>
        /// <returns>New result with updated EWS request ID</returns>
        public IEwsResult<T> WithEwsRequestId(string requestId)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            return new EwsResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                CorrelationId,
                Status,
                RequestId,
                EwsResponseCode,
                requestId,
                new Dictionary<string, object>(Properties));
        }

        /// <summary>
        /// Interface explicit implementation of IEwsResult.WithProperty
        /// </summary>
        IEwsResult IEwsResult.WithProperty(string key, object value)
        {
            return WithProperty(key, value);
        }

        /// <summary>
        /// Interface explicit implementation of IEwsResult.WithProperties
        /// </summary>
        IEwsResult IEwsResult.WithProperties(IDictionary<string, object> properties)
        {
            return WithProperties(properties);
        }

        /// <summary>
        /// Interface explicit implementation of IEwsResult.WithStatus
        /// </summary>
        IEwsResult IEwsResult.WithStatus(OperationStatusType status)
        {
            return WithStatus(status);
        }

        /// <summary>
        /// Interface explicit implementation of IEwsResult.WithRequestId
        /// </summary>
        IEwsResult IEwsResult.WithRequestId(string requestId)
        {
            return WithRequestId(requestId);
        }

        /// <summary>
        /// Interface explicit implementation of IEwsResult.WithEwsResponseCode
        /// </summary>
        IEwsResult IEwsResult.WithEwsResponseCode(EwsResponseCodeType responseCode)
        {
            return WithEwsResponseCode(responseCode);
        }

        /// <summary>
        /// Interface explicit implementation of IEwsResult.WithEwsRequestId
        /// </summary>
        IEwsResult IEwsResult.WithEwsRequestId(string requestId)
        {
            return WithEwsRequestId(requestId);
        }
    }
}