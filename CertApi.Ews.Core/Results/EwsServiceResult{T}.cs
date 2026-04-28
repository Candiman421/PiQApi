// CertApi.Ews.Core/Results/EwsServiceResult{T}.cs

using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Results;
using CertApi.Core.Results;
using CertApi.Ews.Core.Enums;
using CertApi.Ews.Core.Results.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;

namespace CertApi.Ews.Core.Results
{
    /// <summary>
    /// Implementation of generic Ews-specific result that extends the base generic service result
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    public class EwsServiceResult<T> : CertServiceResult<T>, IEwsResult<T>
    {
        /// <summary>
        /// Gets the operation status
        /// </summary>
        public new OperationStatusType Status { get; }

        /// <summary>
        /// Gets the Ews response code if available
        /// </summary>
        public EwsResponseCodeType? EwsResponseCode { get; }

        /// <summary>
        /// Gets the Ews request ID if available
        /// </summary>
        public string? EwsRequestId { get; }

        /// <summary>
        /// Gets the Ews tenant ID if available
        /// </summary>
        public string? TenantId { get; }

        /// <summary>
        /// Gets the Exchange server version
        /// </summary>
        public ExchangeVersion ExchangeVersion { get; }

        /// <summary>
        /// Gets whether throttling was applied to the request
        /// </summary>
        public bool IsThrottled { get; }

        /// <summary>
        /// Gets the Ews service URL used for the request
        /// </summary>
        public Uri? ServiceUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsServiceResult{T}"/> class
        /// </summary>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="value">Result value</param>
        /// <param name="error">Error information</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="timestamp">Timestamp of the result</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="ewsResponseCode">Ews response code</param>
        /// <param name="ewsRequestId">Ews request ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="isThrottled">Whether throttling was applied</param>
        /// <param name="serviceUrl">Service URL</param>
        /// <param name="properties">Additional properties</param>
        protected EwsServiceResult(
            bool isSuccess,
            T? value,
            ICertResultError? error,
            OperationStatusType status,
            string requestId,
            string correlationId,
            DateTimeOffset timestamp,
            ExchangeVersion exchangeVersion,
            EwsResponseCodeType? ewsResponseCode = null,
            string? ewsRequestId = null,
            string? tenantId = null,
            bool isThrottled = false,
            Uri? serviceUrl = null,
            IDictionary<string, object>? properties = null)
            : base(isSuccess, value, error, status, requestId, correlationId, timestamp, properties)
        {
            Status = status;
            EwsResponseCode = ewsResponseCode;
            EwsRequestId = ewsRequestId;
            TenantId = tenantId;
            ExchangeVersion = exchangeVersion;
            IsThrottled = isThrottled;
            ServiceUrl = serviceUrl;
        }

        /// <summary>
        /// Creates a new successful Ews result with a value
        /// </summary>
        /// <param name="value">Result value</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="ewsResponseCode">Ews response code</param>
        /// <param name="ewsRequestId">Ews request ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="serviceUrl">Service URL</param>
        /// <param name="correlationId">Optional correlation ID</param>
        /// <returns>A successful Ews result with value</returns>
        internal static EwsServiceResult<T> CreateSuccess(
            T value,
            OperationStatusType status,
            string requestId,
            ExchangeVersion exchangeVersion,
            EwsResponseCodeType? ewsResponseCode = null,
            string? ewsRequestId = null,
            string? tenantId = null,
            Uri? serviceUrl = null,
            string? correlationId = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Result value cannot be null for success results");
            }

            // Create success marker with correlation ID if provided
            ICertResultError? successMarker = correlationId != null
                ? CertResult.CreateSuccessMarker(correlationId)
                : null;

            return new EwsServiceResult<T>(
                true,
                value,
                successMarker,
                status,
                requestId,
                correlationId ?? string.Empty,
                DateTimeOffset.UtcNow,
                exchangeVersion,
                ewsResponseCode,
                ewsRequestId,
                tenantId,
                false,
                serviceUrl);
        }

        /// <summary>
        /// Creates a new failure Ews result
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Error message</param>
        /// <param name="status">Operation status</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="ewsResponseCode">Ews response code</param>
        /// <param name="ewsRequestId">Ews request ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="isThrottled">Whether throttling was applied</param>
        /// <param name="serviceUrl">Service URL</param>
        /// <param name="correlationId">Optional correlation ID</param>
        /// <returns>A failure Ews result</returns>
        internal static EwsServiceResult<T> CreateFailure(
            string code,
            string message,
            OperationStatusType status,
            string requestId,
            ExchangeVersion exchangeVersion,
            EwsResponseCodeType? ewsResponseCode = null,
            string? ewsRequestId = null,
            string? tenantId = null,
            bool isThrottled = false,
            Uri? serviceUrl = null,
            string? correlationId = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(code);
            ArgumentException.ThrowIfNullOrEmpty(message);
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            var error = new CertError(code, message, correlationId);

            return new EwsServiceResult<T>(
                false,
                default,
                error,
                status,
                requestId,
                correlationId ?? error.CorrelationId,
                DateTimeOffset.UtcNow,
                exchangeVersion,
                ewsResponseCode,
                ewsRequestId,
                tenantId,
                isThrottled,
                serviceUrl);
        }

        /// <summary>
        /// Creates a new Ews result with additional property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        /// <returns>New Ews result with added property</returns>
        public new IEwsResult<T> WithProperty(string key, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            var newProperties = new Dictionary<string, object>(Properties);
            newProperties[key] = value;

            return new EwsServiceResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                Status,
                RequestId,
                CorrelationId,
                Timestamp,
                ExchangeVersion,
                EwsResponseCode,
                EwsRequestId,
                TenantId,
                IsThrottled,
                ServiceUrl,
                newProperties);
        }

        /// <summary>
        /// Creates a new Ews result with additional properties
        /// </summary>
        /// <param name="properties">Properties to add</param>
        /// <returns>New Ews result with added properties</returns>
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

            return new EwsServiceResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                Status,
                RequestId,
                CorrelationId,
                Timestamp,
                ExchangeVersion,
                EwsResponseCode,
                EwsRequestId,
                TenantId,
                IsThrottled,
                ServiceUrl,
                newProperties);
        }

        /// <summary>
        /// Creates a new Ews result with a different status
        /// </summary>
        /// <param name="status">New status</param>
        /// <returns>New Ews result with updated status</returns>
        public new IEwsResult<T> WithStatus(OperationStatusType status)
        {
            return new EwsServiceResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                status,
                RequestId,
                CorrelationId,
                Timestamp,
                ExchangeVersion,
                EwsResponseCode,
                EwsRequestId,
                TenantId,
                IsThrottled,
                ServiceUrl,
                Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        /// <summary>
        /// Creates a new Ews result with a different Ews response code
        /// </summary>
        /// <param name="responseCode">New Ews response code</param>
        /// <returns>New Ews result with updated Ews response code</returns>
        public IEwsResult<T> WithEwsResponseCode(EwsResponseCodeType responseCode)
        {
            return new EwsServiceResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                Status,
                RequestId,
                CorrelationId,
                Timestamp,
                ExchangeVersion,
                responseCode,
                EwsRequestId,
                TenantId,
                IsThrottled,
                ServiceUrl,
                Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        /// <summary>
        /// Creates a new Ews result with a different request ID
        /// </summary>
        /// <param name="requestId">New request ID</param>
        /// <returns>New Ews result with updated request ID</returns>
        public IEwsResult<T> WithRequestId(string requestId)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            return new EwsServiceResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                Status,
                requestId,
                CorrelationId,
                Timestamp,
                ExchangeVersion,
                EwsResponseCode,
                EwsRequestId,
                TenantId,
                IsThrottled,
                ServiceUrl,
                Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        /// <summary>
        /// Creates a new Ews result with a different Ews request ID
        /// </summary>
        /// <param name="requestId">New Ews request ID</param>
        /// <returns>New Ews result with updated Ews request ID</returns>
        public IEwsResult<T> WithEwsRequestId(string requestId)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestId);

            return new EwsServiceResult<T>(
                IsSuccess,
                Value,
                ErrorInfo,
                Status,
                RequestId,
                CorrelationId,
                Timestamp,
                ExchangeVersion,
                EwsResponseCode,
                requestId,
                TenantId,
                IsThrottled,
                ServiceUrl,
                Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
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
        /// Interface explicit implementation of IEwsResult.WithEwsResponseCode
        /// </summary>
        IEwsResult IEwsResult.WithEwsResponseCode(EwsResponseCodeType responseCode)
        {
            return WithEwsResponseCode(responseCode);
        }

        /// <summary>
        /// Interface explicit implementation of IEwsResult.WithRequestId
        /// </summary>
        IEwsResult IEwsResult.WithRequestId(string requestId)
        {
            return WithRequestId(requestId);
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