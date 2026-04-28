// CertApi.Ews.Core/Validation/Interfaces/IEwsValidationContext.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Validation;
using Microsoft.Exchange.WebServices.Data;
using System.Collections.Generic;
using System.Threading;

namespace CertApi.Ews.Core.Validation.Interfaces
{
    /// <summary>
    /// Interface for EWS-specific validation context
    /// Extends the core validation context with EWS-specific properties and methods
    /// </summary>
    public interface IEwsValidationContext : ICertValidationContext
    {
        /// <summary>
        /// Gets the Exchange server version to validate against
        /// </summary>
        ExchangeVersion ExchangeVersion { get; }

        /// <summary>
        /// Gets the EWS scope for validation
        /// </summary>
        string? EwsScope { get; }

        /// <summary>
        /// Gets whether to validate EWS permissions
        /// </summary>
        bool ValidatePermissions { get; }

        /// <summary>
        /// Gets whether to validate EWS-specific extended properties
        /// </summary>
        bool ValidateExtendedProperties { get; }

        /// <summary>
        /// Gets whether to validate EWS well-known folder names
        /// </summary>
        bool ValidateWellKnownFolders { get; }

        /// <summary>
        /// Creates a child validation context for nested validation
        /// </summary>
        /// <returns>A child validation context with incremented depth</returns>
        new IEwsValidationContext CreateChildContext();

        /// <summary>
        /// Creates a new context with a different validation mode
        /// </summary>
        /// <param name="validationMode">New validation mode</param>
        /// <returns>New context with updated validation mode</returns>
        IEwsValidationContext WithValidationMode(ValidationModeType validationMode);

        /// <summary>
        /// Creates a new context with an additional context value
        /// </summary>
        /// <param name="key">Context key</param>
        /// <param name="value">Property value</param>
        /// <returns>New context with added context value</returns>
        new IEwsValidationContext WithContextValue(string key, object value);

        /// <summary>
        /// Creates a new context with additional context values
        /// </summary>
        /// <param name="values">Context values to add</param>
        /// <returns>New context with added context values</returns>
        IEwsValidationContext WithContextValues(IDictionary<string, object> values);

        /// <summary>
        /// Creates a new context with a different EWS scope
        /// </summary>
        /// <param name="ewsScope">New EWS scope</param>
        /// <returns>New context with updated EWS scope</returns>
        IEwsValidationContext WithEwsScope(string ewsScope);

        /// <summary>
        /// Creates a new context with a different value for validating permissions
        /// </summary>
        /// <param name="validatePermissions">New value for validating permissions</param>
        /// <returns>New context with updated validate permissions value</returns>
        IEwsValidationContext WithValidatePermissions(bool validatePermissions);

        /// <summary>
        /// Creates a new context with a different value for validating extended properties
        /// </summary>
        /// <param name="validateExtendedProperties">New value for validating extended properties</param>
        /// <returns>New context with updated validate extended properties value</returns>
        IEwsValidationContext WithValidateExtendedProperties(bool validateExtendedProperties);

        /// <summary>
        /// Creates a new context with a different value for validating well-known folder names
        /// </summary>
        /// <param name="validateWellKnownFolders">New value for validating well-known folder names</param>
        /// <returns>New context with updated validate well-known folder names value</returns>
        IEwsValidationContext WithValidateWellKnownFolders(bool validateWellKnownFolders);

        /// <summary>
        /// Creates a new validation context with a different cancellation token
        /// </summary>
        /// <param name="cancellationToken">New cancellation token</param>
        /// <returns>A new validation context with the updated cancellation token</returns>
        new IEwsValidationContext WithCancellationToken(CancellationToken cancellationToken);
    }
}