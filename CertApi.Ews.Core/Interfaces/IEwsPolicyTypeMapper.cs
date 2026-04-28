// CertApi.Ews.Core/Interfaces/IEwsPolicyTypeMapper.cs
using CertApi.Abstractions.Enums;
using CertApi.Ews.Core.Enums;

namespace CertApi.Ews.Core.Interfaces
{
    /// <summary>
    /// Interface for mapping between Exchange policy types and core resilience policy types
    /// </summary>
    public interface IEwsPolicyTypeMapper
    {
        /// <summary>
        /// Maps Exchange policy type to core resilience policy type
        /// </summary>
        /// <param name="policyType">Exchange policy type</param>
        /// <returns>Core resilience policy type</returns>
        ResiliencePolicyType MapToCorePolicy(EwsPolicyType policyType);
    }
}
