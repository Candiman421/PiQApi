// PiQApi.Ews.Core/Results/EwsResult.cs

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
    /// Implementation of EWS operation result
    /// </summary>
    public class EwsResult : PiQResult, IEwsResult
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
        /// Initializes a new instance of the <see cref="EwsResult"/> class for a successful result
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="ewsResponseCode">Optional EWS response code</param>
        /// <param name="ewsRequestId">Optional EWS request ID</param>
        public EwsResult(
            string correlationId, 
            OperationStatusType status = OperationStatusType.Done, 
            string? requestId = null,
            EwsResponseCodeType? ewsResponseCode = null,
            string? ewsRequestId = null)
            : base(true, null, correlationId, DateTimeOffset.UtcNow, null)
        {
            Status = status;
            RequestId = requestId;
            EwsResponseCode = ewsResponseCode;
            EwsRequestId = ewsRequestId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsResult"/> class for a failed result
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
            : base(false, error, correlationId, DateTimeOffset.UtcNow, null)
        {
            Status = status;
            RequestId = requestId;
            EwsResponseCode = ewsResponseCode;
            EwsRequestId = ewsRequestId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsResult"/> class with custom properties
        /// </summary>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="error">Error information (null for success)</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Optional request ID</param>
        /// <param name="ewsResponseCode">Optional EWS response code</param>
        /// <param name="ewsRequestId">Optional EWS request ID</param>
        /// <param name="properties">Additional properties</param>
        protected EwsResult(
            bool isSuccess,
            IPiQResultError? error,
            string correlationId,
            OperationStatusType status,
            string? requestId,
            EwsResponseCodeType? ewsResponseCode,
            string? ewsRequestId,
            IDictionary<string, object>? properties)
            : base(isSuccess, error, correlationId, DateTimeOffset.UtcNow, properties)
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
        public new IEwsResult WithProperty(string key, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            var newProperties = new Dictionary<string, object>(Properties);
            newProperties[key] = value;

            return new EwsResult(
                IsSuccess,
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
        public new IEwsResult WithProperties(IDictionary<string, object> properties)
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

            return new EwsResult(
                IsSuccess,
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
        public IEwsResult WithStatus(OperationStatusType status)
        {
            return new EwsResult(
                IsSuccess,
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
        public IEwsResult WithRequestId(string requestId)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            return new EwsResult(
                IsSuccess,
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
        public IEwsResult WithEwsResponseCode(EwsResponseCodeType responseCode)
        {
            return new EwsResult(
                IsSuccess,
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
        public IEwsResult WithEwsRequestId(string requestId)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            return new EwsResult(
                IsSuccess,
                ErrorInfo,
                CorrelationId,
                Status,
                RequestId,
                EwsResponseCode,
                requestId,
                new Dictionary<string, object>(Properties));
        }
    }
}