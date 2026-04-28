// PiQApi.Ews.Core/Validation/EwsValidationContext.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Validation;
using PiQApi.Ews.Core.Validation.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace PiQApi.Ews.Core.Validation
{
    /// <summary>
    /// EWS-specific validation context that extends the base validation context
    /// with additional EWS-specific validation properties and behaviors.
    /// </summary>
    public sealed class EwsValidationContext : PiQValidationContext, IEwsValidationContext
    {
        /// <summary>
        /// Gets the Exchange server version to validate against
        /// </summary>
        public ExchangeVersion ExchangeVersion { get; }

        /// <summary>
        /// Gets the EWS scope for validation
        /// </summary>
        public string? EwsScope { get; }

        /// <summary>
        /// Gets whether to validate EWS permissions
        /// </summary>
        public bool ValidatePermissions { get; }

        /// <summary>
        /// Gets whether to validate EWS-specific extended properties
        /// </summary>
        public bool ValidateExtendedProperties { get; }

        /// <summary>
        /// Gets whether to validate EWS well-known folder names
        /// </summary>
        public bool ValidateWellKnownFolders { get; }

        /// <summary>
        /// Creates a new EWS validation context
        /// </summary>
        /// <param name="validationMode">Validation mode</param>
        /// <param name="depth">Current depth</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="aggregateAllErrors">Whether to aggregate all errors</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="additionalContext">Additional context data</param>
        /// <param name="timeProvider">Time provider</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="ewsScope">EWS scope</param>
        /// <param name="validatePermissions">Whether to validate permissions</param>
        /// <param name="validateExtendedProperties">Whether to validate extended properties</param>
        /// <param name="validateWellKnownFolders">Whether to validate well-known folder names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public EwsValidationContext(
            ValidationModeType validationMode,
            int depth,
            int maxDepth,
            bool aggregateAllErrors,
            string correlationId,
            ImmutableDictionary<string, object> additionalContext,
            IPiQTimeProvider timeProvider,
            ExchangeVersion exchangeVersion,
            string? ewsScope = null,
            bool validatePermissions = false,
            bool validateExtendedProperties = true,
            bool validateWellKnownFolders = true,
            CancellationToken cancellationToken = default)
            : base(validationMode, depth, maxDepth, aggregateAllErrors, correlationId, additionalContext, timeProvider, cancellationToken)
        {
            ExchangeVersion = exchangeVersion;
            EwsScope = ewsScope;
            ValidatePermissions = validatePermissions;
            ValidateExtendedProperties = validateExtendedProperties;
            ValidateWellKnownFolders = validateWellKnownFolders;
        }

        /// <summary>
        /// Creates a new EWS validation context with default values
        /// </summary>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public EwsValidationContext(ExchangeVersion exchangeVersion, CancellationToken cancellationToken = default)
            : this(ValidationModeType.Standard, 0, 10, false, Guid.NewGuid().ToString(),
                  ImmutableDictionary<string, object>.Empty, new PiQDefaultTimeProvider(), 
                  exchangeVersion, null, false, true, true, cancellationToken)
        {
        }

        /// <summary>
        /// Creates a new EWS validation context with a correlation ID
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public EwsValidationContext(string correlationId, ExchangeVersion exchangeVersion, CancellationToken cancellationToken = default)
            : this(ValidationModeType.Standard, 0, 10, false, correlationId,
                  ImmutableDictionary<string, object>.Empty, new PiQDefaultTimeProvider(), 
                  exchangeVersion, null, false, true, true, cancellationToken)
        {
        }

        /// <summary>
        /// Creates a new EWS validation context with the specified parameters
        /// </summary>
        /// <param name="validationMode">Validation mode</param>
        /// <param name="exchangeVersion">Exchange server version</param>
        /// <param name="ewsScope">EWS scope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public EwsValidationContext(ValidationModeType validationMode, ExchangeVersion exchangeVersion, string? ewsScope = null, CancellationToken cancellationToken = default)
            : this(validationMode, 0, 10, false, Guid.NewGuid().ToString(),
                  ImmutableDictionary<string, object>.Empty, new PiQDefaultTimeProvider(),
                  exchangeVersion, ewsScope, false, true, true, cancellationToken)
        {
        }

        /// <summary>
        /// Creates a child validation context for nested validation
        /// </summary>
        /// <returns>A child validation context</returns>
        public new IEwsValidationContext CreateChildContext()
        {
            if (Depth >= MaxDepth)
            {
                throw new InvalidOperationException($"Maximum validation depth of {MaxDepth} exceeded");
            }

            return new EwsValidationContext(
                Mode,
                Depth + 1,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                (ImmutableDictionary<string, object>)Context,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                ValidatePermissions,
                ValidateExtendedProperties,
                ValidateWellKnownFolders,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new context with a different validation mode
        /// </summary>
        /// <param name="validationMode">New validation mode</param>
        /// <returns>New context with updated validation mode</returns>
        public IEwsValidationContext WithValidationMode(ValidationModeType validationMode)
        {
            return new EwsValidationContext(
                validationMode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                (ImmutableDictionary<string, object>)Context,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                ValidatePermissions,
                ValidateExtendedProperties,
                ValidateWellKnownFolders,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new context with a different cancellation token
        /// </summary>
        /// <param name="cancellationToken">New cancellation token</param>
        /// <returns>New context with updated cancellation token</returns>
        public new IEwsValidationContext WithCancellationToken(CancellationToken cancellationToken)
        {
            return new EwsValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                (ImmutableDictionary<string, object>)Context,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                ValidatePermissions,
                ValidateExtendedProperties,
                ValidateWellKnownFolders,
                cancellationToken);
        }

        /// <summary>
        /// Creates a new context with an additional context value
        /// </summary>
        /// <param name="key">Context key</param>
        /// <param name="value">Context value</param>
        /// <returns>New context with added context value</returns>
        public new IEwsValidationContext WithContextValue(string key, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            var contextBuilder = (ImmutableDictionary<string, object>)Context;
            var newContext = contextBuilder.SetItem(key, value);

            return new EwsValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                newContext,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                ValidatePermissions,
                ValidateExtendedProperties,
                ValidateWellKnownFolders,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new context with additional context values
        /// </summary>
        /// <param name="values">Context values to add</param>
        /// <returns>New context with added context values</returns>
        public IEwsValidationContext WithContextValues(IDictionary<string, object> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                return this;
            }

            var builder = ((ImmutableDictionary<string, object>)Context).ToBuilder();
            foreach (var kvp in values)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value is not null)
                {
                    builder[kvp.Key] = kvp.Value;
                }
            }

            return new EwsValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                builder.ToImmutable(),
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                ValidatePermissions,
                ValidateExtendedProperties,
                ValidateWellKnownFolders,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new context with a different EWS scope
        /// </summary>
        /// <param name="ewsScope">New EWS scope</param>
        /// <returns>New context with updated EWS scope</returns>
        public IEwsValidationContext WithEwsScope(string ewsScope)
        {
            ArgumentException.ThrowIfNullOrEmpty(ewsScope);

            return new EwsValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                (ImmutableDictionary<string, object>)Context,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                ewsScope,
                ValidatePermissions,
                ValidateExtendedProperties,
                ValidateWellKnownFolders,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new context with a different value for validating permissions
        /// </summary>
        /// <param name="validatePermissions">New value for validating permissions</param>
        /// <returns>New context with updated validate permissions value</returns>
        public IEwsValidationContext WithValidatePermissions(bool validatePermissions)
        {
            return new EwsValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                (ImmutableDictionary<string, object>)Context,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                validatePermissions,
                ValidateExtendedProperties,
                ValidateWellKnownFolders,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new context with a different value for validating extended properties
        /// </summary>
        /// <param name="validateExtendedProperties">New value for validating extended properties</param>
        /// <returns>New context with updated validate extended properties value</returns>
        public IEwsValidationContext WithValidateExtendedProperties(bool validateExtendedProperties)
        {
            return new EwsValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                (ImmutableDictionary<string, object>)Context,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                ValidatePermissions,
                validateExtendedProperties,
                ValidateWellKnownFolders,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new context with a different value for validating well-known folder names
        /// </summary>
        /// <param name="validateWellKnownFolders">New value for validating well-known folder names</param>
        /// <returns>New context with updated validate well-known folder names value</returns>
        public IEwsValidationContext WithValidateWellKnownFolders(bool validateWellKnownFolders)
        {
            return new EwsValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateAllErrors,
                CorrelationId,
                (ImmutableDictionary<string, object>)Context,
                new PiQDefaultTimeProvider(),
                ExchangeVersion,
                EwsScope,
                ValidatePermissions,
                ValidateExtendedProperties,
                validateWellKnownFolders,
                CancellationToken);
        }

        // Explicit interface implementation for IPiQValidationContext methods
        // to ensure they return the more specific IEwsValidationContext

        IPiQValidationContext IPiQValidationContext.CreateChildContext() => CreateChildContext();

        IPiQValidationContext IPiQValidationContext.WithContextValue(string key, object value) => WithContextValue(key, value);

        IPiQValidationContext IPiQValidationContext.WithMode(ValidationModeType mode) => WithValidationMode(mode);

        IPiQValidationContext IPiQValidationContext.WithCancellationToken(CancellationToken cancellationToken) 
            => WithCancellationToken(cancellationToken);
    }
}