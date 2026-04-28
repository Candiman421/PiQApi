// PiQApi.Abstractions/Service/Interfaces/IServiceResponse{T}.cs
// PiQApi.Abstractions/Service/Interfaces/IServiceResponse{T}.cs
// PiQApi.Abstractions/Service/Interfaces/IServiceResponse{T}.cs
namespace PiQApi.Abstractions.Service.Interfaces
{
    /// <summary>
    /// Defines a generic service response with a result of specified type
    /// </summary>
    /// <typeparam name="T">Type of the result</typeparam>
    public interface IServiceResponse<T> : IServiceResponse where T : class
    {
        /// <summary>
        /// Gets the result of the service operation
        /// </summary>
        T? Result { get; }
    }
}