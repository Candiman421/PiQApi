// PiQApi.Ews.Core/Interfaces/Context/IEwsCorrelationContext.cs
using PiQApi.Abstractions.Core;

namespace PiQApi.Ews.Core.Interfaces.Context
{
    /// <summary>
    /// Interface for Exchange Web Services correlation context
    /// Extends the base correlation context with EWS-specific properties and methods
    /// </summary>
    public interface IEwsCorrelationContext : IPiQCorrelationContext
    {
        /// <summary>
        /// Gets the tenant ID associated with the correlation
        /// </summary>
        string? TenantId { get; }

        /// <summary>
        /// Gets the request ID associated with the correlation
        /// </summary>
        string? RequestId { get; }

        /// <summary>
        /// Gets the user principal name associated with the correlation
        /// </summary>
        string? UserPrincipalName { get; }

        /// <summary>
        /// Sets the tenant ID
        /// </summary>
        /// <param name="tenantId">Tenant ID to set</param>
        void SetTenantId(string tenantId);

        /// <summary>
        /// Sets the request ID
        /// </summary>
        /// <param name="requestId">Request ID to set</param>
        void SetRequestId(string requestId);

        /// <summary>
        /// Sets the user principal name
        /// </summary>
        /// <param name="userPrincipalName">User principal name to set</param>
        void SetUserPrincipalName(string userPrincipalName);

        /// <summary>
        /// Creates a correlation scope with this context
        /// </summary>
        /// <param name="additionalProperties">Additional properties to add to the scope</param>
        /// <returns>A disposable scope</returns>
        IDisposable CreateScope(IDictionary<string, object>? additionalProperties = null);
    }
}