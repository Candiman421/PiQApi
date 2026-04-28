// CertApi.Ews.Service/Core/EwsPolicyTypeMapper.cs
using CertApi.Abstractions.Enums;
using CertApi.Ews.Core.Enums;
using CertApi.Ews.Core.Interfaces;
using CertApi.Ews.Service.Core.Options;
using Microsoft.Extensions.Options;

namespace CertApi.Ews.Service.Core
{
    /// <summary>
    /// Maps between Exchange policy types and core resilience policy types
    /// </summary>
    public class EwsPolicyTypeMapper : IEwsPolicyTypeMapper
    {
        private readonly IReadOnlyDictionary<EwsPolicyType, ResiliencePolicyType> _policyTypeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsPolicyTypeMapper"/> class
        /// </summary>
        /// <param name="options">Policy mapping options</param>
        public EwsPolicyTypeMapper(IOptions<EwsPolicyMappingOptions>? options = null)
        {
            _policyTypeMap = options?.Value?.PolicyTypeMap ?? CreateDefaultMappings();
        }

        /// <summary>
        /// Maps Exchange policy type to core resilience policy type
        /// </summary>
        public ResiliencePolicyType MapToCorePolicy(EwsPolicyType EwsPolicyType)
        {
            return _policyTypeMap.TryGetValue(policyType, out var corePolicyType)
                ? corePolicyType
                : ResiliencePolicyType.Default;
        }

        private static Dictionary<EwsPolicyType, ResiliencePolicyType> CreateDefaultMappings()
        {
            return new Dictionary<EwsPolicyType, ResiliencePolicyType>
            {
                { EwsPolicyType.Default, ResiliencePolicyType.Default },
                { EwsPolicyType.Service, ResiliencePolicyType.Default },
                { EwsPolicyType.Authentication, ResiliencePolicyType.Authentication },
                { EwsPolicyType.Mail, ResiliencePolicyType.Default },
                { EwsPolicyType.Batch, ResiliencePolicyType.Bulkhead },
                { EwsPolicyType.Search, ResiliencePolicyType.Timeout },
                { EwsPolicyType.Validation, ResiliencePolicyType.Default }
            };
        }
    }
}
