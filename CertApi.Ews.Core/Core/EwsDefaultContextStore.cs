// CertApi.Ews.Core/Core/EwsDefaultContextStore.cs
using CertApi.Abstractions.Core;
using CertApi.Ews.Core.Core.Interfaces;

namespace CertApi.Ews.Core.Core
{
    /// <summary>
    /// Default implementation of IEwsContextStore using AsyncLocal
    /// </summary>
    public class EwsDefaultContextStore : IEwsContextStore
    {
        /// <summary>
        /// Gets the current Ews correlation context from AsyncLocal storage
        /// </summary>
        /// <returns>The current context or null if not set</returns>
        public EwsCorrelationContext? GetCurrentContext() => EwsCorrelationContext.Current;

        /// <summary>
        /// Gets the current correlation context from AsyncLocal storage
        /// Implements ICertContextStore.GetCurrentContext
        /// </summary>
        /// <returns>The current context as ICertCorrelationContext or null if not set</returns>
        ICertCorrelationContext? ICertContextStore.GetCurrentContext() => GetCurrentContext();

        /// <summary>
        /// Sets the current Ews correlation context in AsyncLocal storage
        /// </summary>
        /// <param name="context">The context to set as current</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        public void SetCurrentContext(EwsCorrelationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            EwsCorrelationContext.SetCurrent(context);
        }

        /// <summary>
        /// Sets the current correlation context in AsyncLocal storage
        /// Implements ICertContextStore.SetCurrentContext
        /// </summary>
        /// <param name="context">The context to set as current</param>
        /// <exception cref="ArgumentException">Thrown when context is not an EwsCorrelationContext</exception>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        void ICertContextStore.SetCurrentContext(ICertCorrelationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            
            if (context is EwsCorrelationContext ewsContext)
            {
                SetCurrentContext(ewsContext);
            }
            else
            {
                throw new ArgumentException($"Context must be of type {nameof(EwsCorrelationContext)}", nameof(context));
            }
        }
    }
}