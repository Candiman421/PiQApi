// CertApi.Abstractions/Service/Interfaces/IServiceResponse{T}.cs
// CertApi.Abstractions/Service/Interfaces/IServiceResponse{T}.cs
// CertApi.Abstractions/Service/Interfaces/IServiceResponse{T}.cs
namespace CertApi.Abstractions.Service.Interfaces
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