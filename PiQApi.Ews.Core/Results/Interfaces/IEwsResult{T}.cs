// PiQApi.Ews.Core/Results/Interfaces/IEwsResult{T}.cs

using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Results;
using PiQApi.Ews.Core.Enums;
using System.Collections.Generic;

namespace PiQApi.Ews.Core.Results.Interfaces
{
    /// <summary>
    /// Interface for EWS operation results with a value
    /// </summary>
    /// <typeparam name="T">Type of result value</typeparam>
    public interface IEwsResult<out T> : IEwsResult, ICertResult<T>
    {
        /// <summary>
        /// Creates a new result with additional property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        /// <returns>New result with added property</returns>
        new IEwsResult<T> WithProperty(string key, object value);

        /// <summary>
        /// Creates a new result with additional properties
        /// </summary>
        /// <param name="properties">Properties to add</param>
        /// <returns>New result with added properties</returns>
        new IEwsResult<T> WithProperties(IDictionary<string, object> properties);

        /// <summary>
        /// Creates a new result with different status
        /// </summary>
        /// <param name="status">New status</param>
        /// <returns>New result with updated status</returns>
        new IEwsResult<T> WithStatus(OperationStatusType status);

        /// <summary>
        /// Creates a new result with different request ID
        /// </summary>
        /// <param name="requestId">New request ID</param>
        /// <returns>New result with updated request ID</returns>
        new IEwsResult<T> WithRequestId(string requestId);

        /// <summary>
        /// Creates a new result with different EWS response code
        /// </summary>
        /// <param name="responseCode">New response code</param>
        /// <returns>New result with updated response code</returns>
        new IEwsResult<T> WithEwsResponseCode(EwsResponseCodeType responseCode);

        /// <summary>
        /// Creates a new result with different EWS request ID
        /// </summary>
        /// <param name="requestId">New EWS request ID</param>
        /// <returns>New result with updated EWS request ID</returns>
        new IEwsResult<T> WithEwsRequestId(string requestId);
    }
}
