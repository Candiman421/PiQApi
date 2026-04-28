// PiQApi.Ews.Service/Core/Options/EwsConnectionOptions.cs
using PiQApi.Core.Configuration;

namespace PiQApi.Ews.Service.Core.Options
{
    /// <summary>
    /// Options for EWS connection management
    /// </summary>
    public class EwsConnectionOptions : PiQConnectionOptions
    {
        /// <summary>
        /// Gets or sets whether circuit breaker is enabled
        /// </summary>
        public bool EnableCircuitBreaker { get; set; } = true;

        /// <summary>
        /// Gets or sets the circuit breaker failure threshold (0-1)
        /// </summary>
        public double CircuitBreakerThreshold { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the circuit breaker duration
        /// </summary>
        public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets whether connection pooling is enabled
        /// </summary>
        public bool EnablePooling { get; set; } = true;

        /// <summary>
        /// Gets or sets whether connections are validated on acquire
        /// </summary>
        public bool ValidateOnAcquire { get; set; } = true;

        /// <summary>
        /// Gets or sets whether connections are validated on release
        /// </summary>
        public bool ValidateOnRelease { get; set; }
    }
}
