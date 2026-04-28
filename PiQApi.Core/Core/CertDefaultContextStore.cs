// PiQApi.Core/Core/CertDefaultContextStore.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Core.Core
{
    /// <summary>
    /// Default implementation of ICertContextStore using AsyncLocal storage
    /// </summary>
    public class CertDefaultContextStore : ICertContextStore
    {
        /// <summary>
        /// Gets the current correlation context from AsyncLocal storage
        /// </summary>
        /// <returns>The current context or null if not set</returns>
        public ICertCorrelationContext? GetCurrentContext() => CertCorrelationContext.Current;

        /// <summary>
        /// Sets the current correlation context in AsyncLocal storage
        /// </summary>
        /// <param name="context">The context to set as current</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        /// <exception cref="ArgumentException">Thrown when context is not of type CertCorrelationContext</exception>
        public void SetCurrentContext(ICertCorrelationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context is CertCorrelationContext certContext)
            {
                CertCorrelationContext.SetCurrent(certContext);
            }
            else
            {
                throw new ArgumentException($"Context must be of type {nameof(CertCorrelationContext)}", nameof(context));
            }
        }
    }
}