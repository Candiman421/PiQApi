// CertApi.Abstractions/Results/ICertServiceResult{T}.cs

using CertApi.Abstractions.Enums;

namespace CertApi.Abstractions.Results;

/// <summary>
/// Defines a service result containing a value
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public interface ICertServiceResult<out T> : ICertServiceResult, ICertResult<T>
{
    /// <summary>
    /// Creates a new service result with additional property
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    /// <returns>New service result with added property</returns>
    new ICertServiceResult<T> WithProperty(string key, object value);

    /// <summary>
    /// Creates a new service result with additional properties
    /// </summary>
    /// <param name="properties">Properties to add</param>
    /// <returns>New service result with added properties</returns>
    new ICertServiceResult<T> WithProperties(IDictionary<string, object> properties);

    /// <summary>
    /// Creates a new service result with a different status
    /// </summary>
    /// <param name="status">New status</param>
    /// <returns>New service result with updated status</returns>
    new ICertServiceResult<T> WithStatus(OperationStatusType status);
}
