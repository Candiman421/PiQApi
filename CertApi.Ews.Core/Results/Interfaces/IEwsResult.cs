// CertApi.Ews.Core/Results/Interfaces/IEwsResult.cs

using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Results;
using CertApi.Ews.Core.Enums;
using System.Collections.Generic;

namespace CertApi.Ews.Core.Results.Interfaces
{
    /// <summary>
    /// Base interface for all EWS operation results
    /// </summary>
    public interface IEwsResult : ICertResult
    {
        /// <summary>
        /// Gets the operation status
        /// </summary>
        OperationStatusType Status { get; }

        /// <summary>
        /// Gets the request ID if available
        /// </summary>
        string? RequestId { get; }

        /// <summary>
        /// Creates a new result with additional property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        /// <returns>New result with added property</returns>
        new IEwsResult WithProperty(string key, object value);

        /// <summary>
        /// Creates a new result with additional properties
        /// </summary>
        /// <param name="properties">Properties to add</param>
        /// <returns>New result with added properties</returns>
        new IEwsResult WithProperties(IDictionary<string, object> properties);

        /// <summary>
        /// Creates a new result with different status
        /// </summary>
        /// <param name="status">New status</param>
        /// <returns>New result with updated status</returns>
        IEwsResult WithStatus(OperationStatusType status);

        /// <summary>
        /// Creates a new result with different request ID
        /// </summary>
        /// <param name="requestId">New request ID</param>
        /// <returns>New result with updated request ID</returns>
        IEwsResult WithRequestId(string requestId);
        
        /// <summary>
        /// Creates a new result with different EWS response code
        /// </summary>
        /// <param name="responseCode">New response code</param>
        /// <returns>New result with updated response code</returns>
        IEwsResult WithEwsResponseCode(EwsResponseCodeType responseCode);
        
        /// <summary>
        /// Creates a new result with different EWS request ID
        /// </summary>
        /// <param name="requestId">New EWS request ID</param>
        /// <returns>New result with updated EWS request ID</returns>
        IEwsResult WithEwsRequestId(string requestId);
    }
}