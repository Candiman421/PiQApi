// CertApi.Ews.Core/Core/EwsCorrelationContextOptions.cs
using CertApi.Core.Configuration;

namespace CertApi.Ews.Core.Core
{
    /// <summary>
    /// Options for configuring Exchange correlation context
    /// </summary>
    public class EwsCorrelationContextOptions
    {
        /// <summary>
        /// Gets the base correlation context options
        /// </summary>
        public CertCorrelationContextOptions BaseOptions { get; }

        /// <summary>
        /// Gets or sets the default tenant ID
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets additional properties to add to Exchange correlation context
        /// </summary>
        public IDictionary<string, object> AdditionalProperties => _additionalProperties;

        private readonly Dictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsCorrelationContextOptions"/> class
        /// </summary>
        public EwsCorrelationContextOptions()
        {
            BaseOptions = new CertCorrelationContextOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsCorrelationContextOptions"/> class with base options
        /// </summary>
        /// <param name="baseOptions">Base correlation context options</param>
        public EwsCorrelationContextOptions(CertCorrelationContextOptions baseOptions)
        {
            BaseOptions = baseOptions ?? new CertCorrelationContextOptions();
        }

        /// <summary>
        /// Adds a property to the additional properties collection
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void AddProperty(string key, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            _additionalProperties[key] = value;
        }

        /// <summary>
        /// Adds multiple properties to the additional properties collection
        /// </summary>
        /// <param name="properties">Properties to add</param>
        public void AddProperties(IDictionary<string, object> properties)
        {
            ArgumentNullException.ThrowIfNull(properties);

            foreach (var kvp in properties)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
                {
                    _additionalProperties[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}