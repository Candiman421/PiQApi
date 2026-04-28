// PiQApi.Ews.Core/Enums/EwsPolicyType.cs
namespace PiQApi.Ews.Core.Enums
{
    /// <summary>
    /// Exchange-specific policy types for resilience
    /// </summary>
    public enum EwsPolicyType
    {
        /// <summary>
        /// Default policy for general operations
        /// </summary>
        Default,

        /// <summary>
        /// Policy for service operations
        /// </summary>
        Service,

        /// <summary>
        /// Policy for authentication operations
        /// </summary>
        Authentication,

        /// <summary>
        /// Policy for mail operations
        /// </summary>
        Mail,

        /// <summary>
        /// Policy for batch operations
        /// </summary>
        Batch,

        /// <summary>
        /// Policy for search operations
        /// </summary>
        Search,

        /// <summary>
        /// Policy for validation operations
        /// </summary>
        Validation
    }
}
