// PiQApi.Abstractions/Validation/IPiQValidationContext.cs
using PiQApi.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PiQApi.Abstractions.Validation
{
    /// <summary>
    /// Interface for validation context providing metadata and configuration for validation operations
    /// </summary>
    public interface IPiQValidationContext
    {
        /// <summary>
        /// Gets the validation mode
        /// </summary>
        ValidationModeType Mode { get; }

        /// <summary>
        /// Gets the current validation depth (for nested validations)
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Gets the maximum validation depth allowed
        /// </summary>
        int MaxDepth { get; }

        /// <summary>
        /// Gets whether to aggregate all errors or stop on first error
        /// </summary>
        bool AggregateAllErrors { get; }

        /// <summary>
        /// Gets the correlation ID for tracking validation
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Gets the context dictionary containing additional validation properties
        /// </summary>
        IReadOnlyDictionary<string, object> Context { get; }

        /// <summary>
        /// Gets the validation timestamp
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the cancellation token
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Tries to get a value from the context
        /// </summary>
        /// <typeparam name="T">Type of value to retrieve</typeparam>
        /// <param name="key">Key to lookup</param>
        /// <param name="value">Value if found and correct type</param>
        /// <returns>True if value was found and is of correct type</returns>
        bool TryGetValue<T>(string key, out T? value);

        /// <summary>
        /// Gets a value from the context
        /// </summary>
        /// <typeparam name="T">Type of value to retrieve</typeparam>
        /// <param name="key">Key to lookup</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Value if found and correct type, otherwise defaultValue</returns>
        T? GetValue<T>(string key, T? defaultValue = default);

        /// <summary>
        /// Gets a string value from the context
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>String value if found, otherwise defaultValue</returns>
        string GetString(string key, string defaultValue = "");

        /// <summary>
        /// Gets an integer value from the context
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Integer value if found, otherwise defaultValue</returns>
        int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// Gets a boolean value from the context
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Boolean value if found, otherwise defaultValue</returns>
        bool GetBool(string key, bool defaultValue = false);

        /// <summary>
        /// Checks if a key exists in the context
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if key exists</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Creates a child validation context with incremented depth
        /// </summary>
        /// <returns>New validation context with depth + 1</returns>
        IPiQValidationContext CreateChildContext();

        /// <summary>
        /// Creates a new validation context with an additional context property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        /// <returns>A new validation context with the added property</returns>
        IPiQValidationContext WithContextValue(string key, object value);

        /// <summary>
        /// Creates a new validation context with a different validation mode
        /// </summary>
        /// <param name="mode">New validation mode</param>
        /// <returns>A new validation context with the updated mode</returns>
        IPiQValidationContext WithMode(ValidationModeType mode);

        /// <summary>
        /// Creates a new validation context with a different cancellation token
        /// </summary>
        /// <param name="cancellationToken">New cancellation token</param>
        /// <returns>A new validation context with the updated cancellation token</returns>
        IPiQValidationContext WithCancellationToken(CancellationToken cancellationToken);
    }
}