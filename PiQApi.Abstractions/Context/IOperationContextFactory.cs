// PiQApi.Abstractions/Context/IOperationContextFactory.cs
using PiQApi.Abstractions.Core.Interfaces;

namespace PiQApi.Abstractions.Context
{
    /// <summary>
    /// Factory for creating operation contexts
    /// </summary>
    public interface IOperationContextFactory
    {
        /// <summary>
        /// Creates a new operation context with the specified operation name and correlation context
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="correlationContext">Correlation context for distributed tracing</param>
        /// <returns>A new operation context</returns>
        IOperationContext Create(string operationName, ICorrelationContext correlationContext);

        /// <summary>
        /// Creates a new operation context with additional properties
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="correlationContext">Correlation context for distributed tracing</param>
        /// <param name="properties">Additional properties to initialize the context with</param>
        /// <returns>A new operation context with the specified properties</returns>
        IOperationContext CreateWithProperties(string operationName, ICorrelationContext correlationContext, IDictionary<string, object> properties);
    }
}