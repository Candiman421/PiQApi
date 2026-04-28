// CertApi.Ews.Service/Core/Options/EwsPolicyMappingOptions.cs
using CertApi.Abstractions.Enums;
using CertApi.Ews.Core.Enums;

namespace CertApi.Ews.Service.Core.Options
{
    /// <summary>
    /// Options for policy type mapping
    /// </summary>
    public class EwsPolicyMappingOptions
    {
        /// <summary>
        /// Gets or sets the policy type map
        /// </summary>
        public Dictionary<EwsPolicyType, ResiliencePolicyType> EwsPolicyTypeMap { get; set; } = new();
    }
}
