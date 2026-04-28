// PiQApi.Abstractions/Service/IPiQServiceResponse{T}.cs
namespace PiQApi.Abstractions.Service;

/// <summary>
/// Defines a generic service response with a result of specified type
/// </summary>
/// <typeparam name="T">Type of the result</typeparam>
public interface IPiQServiceResponse<T> : IPiQServiceResponse where T : class
{
    /// <summary>
    /// Gets the strongly-typed result of the service operation
    /// </summary>
    new T? Result { get; }
}
