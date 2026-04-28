// PiQApi.Abstractions/Results/IPiQServiceResult.cs

using PiQApi.Abstractions.Enums;

namespace PiQApi.Abstractions.Results;

/// <summary>
/// Defines a service result with operation status
/// </summary>
public interface IPiQServiceResult : IPiQResult
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
    new IPiQServiceResult WithProperty(string key, object value);

    /// <summary>
    /// Creates a new service result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New service result with added properties</returns>
    new IPiQServiceResult WithProperties(IDictionary<string, object> properties);

    /// <summary>
    /// Creates a new service result with a different status
    /// </summary>
    /// <param name="status">New status</param>
    /// <returns>New service result with updated status</returns>
    IPiQServiceResult WithStatus(OperationStatusType status);
}
