// CertApi.Core/Validation/CertValidationContext.cs
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Utilities.Time;
using CertApi.Abstractions.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace CertApi.Core.Validation
{
    /// <summary>
    /// Implementation of the validation context
    /// </summary>
    public class CertValidationContext : ICertValidationContext, ICancellableContext
    {
        private readonly ImmutableDictionary<string, object> _contextData;
        private readonly ICertTimeProvider _timeProvider;

        /// <summary>
        /// Gets the validation mode
        /// </summary>
        public ValidationModeType Mode { get; }

        /// <summary>
        /// Gets the current depth in the validation hierarchy
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Gets the maximum depth allowed for validation
        /// </summary>
        public int MaxDepth { get; }

        /// <summary>
        /// Gets whether to aggregate errors
        /// </summary>
        public bool AggregateErrors { get; }

        /// <summary>
        /// Gets whether to aggregate all errors or stop on first error
        /// </summary>
        public bool AggregateAllErrors => AggregateErrors;

        /// <summary>
        /// Gets whether to stop on first failure
        /// </summary>
        public bool StopOnFirstFailure => !AggregateErrors;

        /// <summary>
        /// Gets the correlation ID
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// Gets the validation timestamp
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the validation timestamp as DateTime
        /// </summary>
        DateTime ICertValidationContext.Timestamp => Timestamp.UtcDateTime;

        /// <summary>
        /// Gets the cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets the dictionary of context values
        /// </summary>
        public IReadOnlyDictionary<string, object> Values => _contextData;

        /// <summary>
        /// Gets the dictionary of context values
        /// </summary>
        public IReadOnlyDictionary<string, object> Context => _contextData;

        /// <summary>
        /// Gets the validation mode (alias for Mode)
        /// </summary>
        public ValidationModeType ValidationMode => Mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertValidationContext"/> class
        /// </summary>
        /// <param name="mode">Validation mode</param>
        /// <param name="depth">Current depth</param>
        /// <param name="maxDepth">Maximum depth</param>
        /// <param name="aggregateErrors">Whether to aggregate errors</param>
        /// <param name="correlationId">Correlation ID</param>
        /// <param name="contextData">Context data</param>
        /// <param name="timeProvider">Time provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public CertValidationContext(
            ValidationModeType mode,
            int depth,
            int maxDepth,
            bool aggregateErrors,
            string correlationId,
            ImmutableDictionary<string, object> contextData,
            ICertTimeProvider timeProvider,
            CancellationToken cancellationToken = default)
        {
            Mode = mode;
            Depth = depth;
            MaxDepth = maxDepth;
            AggregateErrors = aggregateErrors;
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            _contextData = contextData ?? ImmutableDictionary<string, object>.Empty;
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            Timestamp = _timeProvider.UtcNow;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Creates a child context
        /// </summary>
        /// <param name="contextData">Additional context data</param>
        /// <returns>A child context</returns>
        public ICertValidationContext CreateChildContext(ImmutableDictionary<string, object>? contextData = null)
        {
            // Increment depth for child context
            var newDepth = Depth + 1;

            // Check if we've exceeded max depth
            if (newDepth > MaxDepth)
            {
                throw new InvalidOperationException($"Maximum validation depth of {MaxDepth} exceeded");
            }

            // Merge existing context data with new data if provided
            var newContextData = contextData != null
                ? _contextData.SetItems(contextData)
                : _contextData;

            // Create new child context with incremented depth
            return new CertValidationContext(
                Mode,
                newDepth,
                MaxDepth,
                AggregateErrors,
                CorrelationId,
                newContextData,
                _timeProvider,
                CancellationToken);
        }

        /// <summary>
        /// Creates a child context
        /// </summary>
        /// <returns>A child context</returns>
        public ICertValidationContext CreateChildContext()
        {
            return CreateChildContext(null);
        }

        /// <summary>
        /// Gets a value from the context
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Value or default</returns>
        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            if (_contextData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Tries to get a value from the context
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Output value</param>
        /// <returns>True if value exists and is of the correct type, otherwise false</returns>
        public bool TryGetValue<T>(string key, out T? value)
        {
            value = default;

            if (string.IsNullOrEmpty(key) || !_contextData.TryGetValue(key, out var contextValue))
            {
                return false;
            }

            if (contextValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            try
            {
                value = (T?)Convert.ChangeType(contextValue, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a string value from the context
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>String value if found, otherwise defaultValue</returns>
        public string GetString(string key, string defaultValue = "")
        {
            return GetValue<string>(key, defaultValue) ?? defaultValue;
        }

        /// <summary>
        /// Gets an integer value from the context
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Integer value if found, otherwise defaultValue</returns>
        public int GetInt(string key, int defaultValue = 0)
        {
            return GetValue<int>(key, defaultValue);
        }

        /// <summary>
        /// Gets a boolean value from the context
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Boolean value if found, otherwise defaultValue</returns>
        public bool GetBool(string key, bool defaultValue = false)
        {
            return GetValue<bool>(key, defaultValue);
        }

        /// <summary>
        /// Checks if a key exists in the context
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if key exists</returns>
        public bool ContainsKey(string key)
        {
            return !string.IsNullOrEmpty(key) && _contextData.ContainsKey(key);
        }

        /// <summary>
        /// Creates a new validation context with an additional context property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        /// <returns>A new validation context with the added property</returns>
        public ICertValidationContext WithContextValue(string key, object value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            // Create a new dictionary with the additional value
            var newContextData = _contextData.Add(key, value);

            // Return a new context with the updated dictionary
            return new CertValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateErrors,
                CorrelationId,
                newContextData,
                _timeProvider,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new validation context with a different validation mode
        /// </summary>
        /// <param name="mode">New validation mode</param>
        /// <returns>A new validation context with the updated mode</returns>
        public ICertValidationContext WithMode(ValidationModeType mode)
        {
            // Return a new context with the updated mode
            return new CertValidationContext(
                mode,
                Depth,
                MaxDepth,
                mode == ValidationModeType.Lenient, // Aggregate errors in lenient mode
                CorrelationId,
                _contextData,
                _timeProvider,
                CancellationToken);
        }

        /// <summary>
        /// Creates a new validation context with a different cancellation token
        /// </summary>
        /// <param name="cancellationToken">New cancellation token</param>
        /// <returns>A new validation context with the updated cancellation token</returns>
        public ICertValidationContext WithCancellationToken(CancellationToken cancellationToken)
        {
            // Return a new context with the updated cancellation token
            return new CertValidationContext(
                Mode,
                Depth,
                MaxDepth,
                AggregateErrors,
                CorrelationId,
                _contextData,
                _timeProvider,
                cancellationToken);
        }

        /// <summary>
        /// Starts a timer for the specified operation
        /// </summary>
        /// <param name="operationKey">Operation key</param>
        /// <returns>Start time</returns>
        public DateTimeOffset StartTimer(string operationKey)
        {
            return _timeProvider.UtcNow;
        }

        /// <summary>
        /// Stops a timer and calculates the duration
        /// </summary>
        /// <param name="operationKey">Operation key</param>
        /// <param name="startTime">Start time</param>
        /// <returns>Duration</returns>
        public TimeSpan StopTimer(string operationKey, DateTimeOffset startTime)
        {
            return _timeProvider.UtcNow - startTime;
        }
    }
}
