// PiQApi.Ews.Service/Core/Options/EwsPolicyMappingOptions.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Ews.Core.Enums;

namespace PiQApi.Ews.Service.Core.Options
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
