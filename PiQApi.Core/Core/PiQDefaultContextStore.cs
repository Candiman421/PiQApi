// PiQApi.Core/Core/PiQDefaultContextStore.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Core.Core
{
    /// <summary>
    /// Default implementation of IPiQContextStore using AsyncLocal storage
    /// </summary>
    public class PiQDefaultContextStore : IPiQContextStore
    {
        /// <summary>
        /// Gets the current correlation context from AsyncLocal storage
        /// </summary>
        /// <returns>The current context or null if not set</returns>
        public IPiQCorrelationContext? GetCurrentContext() => PiQCorrelationContext.Current;

        /// <summary>
        /// Sets the current correlation context in AsyncLocal storage
        /// </summary>
        /// <param name="context">The context to set as current</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        /// <exception cref="ArgumentException">Thrown when context is not of type PiQCorrelationContext</exception>
        public void SetCurrentContext(IPiQCorrelationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context is PiQCorrelationContext certContext)
            {
                PiQCorrelationContext.SetCurrent(certContext);
            }
            else
            {
                throw new ArgumentException($"Context must be of type {nameof(PiQCorrelationContext)}", nameof(context));
            }
        }
    }
}