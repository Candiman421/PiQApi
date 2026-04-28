// PiQApi.Abstractions/Results/ICertServiceResult.cs

using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Results;

/// <summary>
/// Defines a service result with operation status
/// </summary>
public interface ICertServiceResult : ICertResult
{
    /// <summary>
    /// Gets the operation status
    /// </summary>
    OperationStatusType Status { get; }

    /// <summary>
    /// Gets the request ID
    /// </summary>
    string RequestId { get; }

    /// <summary>
    /// Creates a new service result with additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New service result with added property</returns>
    new ICertServiceResult WithProperty(string key, object value);

    /// <summary>
    /// Creates a new service result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New service result with added properties</returns>
    new ICertServiceResult WithProperties(IDictionary<string, object> properties);

    /// <summary>
    /// Creates a new service result with a different status
    /// </summary>
    /// <param name="status">New status</param>
    /// <returns>New service result with updated status</returns>
    ICertServiceResult WithStatus(OperationStatusType status);
}
