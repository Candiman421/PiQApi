// PiQApi.Ews.Core/Core/Interfaces/IEwsContextStore.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Ews.Core.Core.Interfaces
{
    /// <summary>
    /// Interface for storing and retrieving the current EWS correlation context
    /// Extends the base context store interface for EWS-specific functionality
    /// </summary>
    public interface IEwsContextStore : ICertContextStore
    {
        /// <summary>
        /// Gets the current EWS correlation context
        /// </summary>
        /// <returns>The current EWS context or null if not set</returns>
        new EwsCorrelationContext? GetCurrentContext();

        /// <summary>
        /// Sets the current EWS correlation context
        /// </summary>
        /// <param name="context">The EWS correlation context to set as current</param>
        void SetCurrentContext(EwsCorrelationContext context);
    }
}