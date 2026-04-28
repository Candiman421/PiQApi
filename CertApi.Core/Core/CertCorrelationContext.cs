// CertApi.Core/Core/CertCorrelationContext.cs
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Factories;
using CertApi.Core.Configuration;
using CertApi.Core.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace CertApi.Core.Core;

/// <summary>
/// Implementation of correlation context that propagates correlation IDs through execution flow
/// </summary>
public sealed partial class CertCorrelationContext : ICertCorrelationContext
{
    // Using AsyncLocal ensures context flows across async boundaries
    internal static readonly AsyncLocal<CertCorrelationContext?> _current = new AsyncLocal<CertCorrelationContext?>();

    private readonly ICertCorrelationIdFactory _correlationIdFactory;
    private readonly ILogger<CertCorrelationContext> _logger;
    private readonly ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();
    private readonly Stack<CertCorrelationScopeState> _scopeStack = new Stack<CertCorrelationScopeState>();
    private readonly CertCorrelationContextOptions _options;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, object, Exception?> LogPropertyAdded =
        LoggerMessage.Define<string, object>(
            LogLevel.Debug,
            new EventId(1, nameof(AddProperty)),
            "Added correlation property: {Key}={Value}");

    private static readonly Action<ILogger, string, Exception?> LogParentCorrelationSet =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, nameof(SetParentCorrelation)),
            "Set parent correlation ID: {ParentId}");

    private static readonly Action<ILogger, string, Exception?> LogCorrelationIdSet =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(3, nameof(SetCorrelationId)),
            "Set current correlation ID: {CertCorrelationId}");

    private static readonly Action<ILogger, string, Exception?> LogInitialCorrelationCreated =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(8, "GetOrCreateCorrelationId"),
            "Created new correlation ID on first access: {CertCorrelationId}");

    private static readonly Action<ILogger, string, Exception?> LogCurrentContextSet =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(9, "SetCurrent"),
            "Set current correlation context with ID: {CorrelationId}");

    /// <summary>
    /// Gets the current correlation context from AsyncLocal storage
    /// </summary>
    public static CertCorrelationContext? Current => _current.Value;

    /// <summary>
    /// Sets the current correlation context in AsyncLocal storage
    /// </summary>
    /// <param name="context">The correlation context to set as current, or null to clear it</param>
    public static void SetCurrent(CertCorrelationContext? context)
    {
        _current.Value = context;

        // If context is being set, log it
        if (context != null && context._logger != null)
        {
            LogCurrentContextSet(context._logger, context.CorrelationId, null);
        }
    }

    /// <summary>
    /// Gets the unique correlation identifier
    /// </summary>
    public string CorrelationId => GetOrCreateCorrelationId().Id;

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTime CreatedUtc => GetOrCreateCorrelationId().CreatedUtc;

    /// <summary>
    /// Gets the parent correlation identifier, if any
    /// </summary>
    public string? ParentCorrelationId { get; private set; }

    /// <summary>
    /// Gets the collection of correlation properties
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Creates a new correlation context
    /// </summary>
    /// <param name="correlationIdFactory">Factory for creating correlation IDs</param>
    /// <param name="logger">Logger</param>
    /// <param name="options">Configuration options</param>
    public CertCorrelationContext(
        ICertCorrelationIdFactory correlationIdFactory,
        ILogger<CertCorrelationContext> logger,
        IOptions<CertCorrelationContextOptions>? options = null)
    {
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new CertCorrelationContextOptions();
    }

    /// <summary>
    /// Creates a correlation context with existing correlation ID
    /// </summary>
    /// <param name="correlationId">The correlation ID</param>
    /// <param name="correlationIdFactory">Factory for creating correlation IDs</param>
    /// <param name="logger">Logger</param>
    /// <param name="options">Configuration options</param>
    public CertCorrelationContext(
        ICertCorrelationId correlationId,
        ICertCorrelationIdFactory correlationIdFactory,
        ILogger<CertCorrelationContext> logger,
        IOptions<CertCorrelationContextOptions>? options = null)
    {
        ArgumentNullException.ThrowIfNull(correlationId);
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new CertCorrelationContextOptions();

        // Set the correlation ID in AsyncLocal storage
        SetCurrent(this);

        // Set the properties from the correlation ID
        foreach (var property in correlationId.Properties)
        {
            _properties[property.Key] = property.Value;
        }
    }

    /// <summary>
    /// Gets or creates a correlation ID for this context
    /// </summary>
    /// <returns>A concrete CertCorrelationId instance</returns>
    /// <remarks>
    /// This method ensures the context has a valid correlation ID by:
    /// 1. Checking if there is a current context, creating one if needed
    /// 2. Checking if the current context has a correlation ID, creating one if needed
    /// 3. Converting any interface-based correlation ID to a concrete type
    /// </remarks>
    private CertCorrelationId GetOrCreateCorrelationId()
    {
        // If there's no current context set, set this instance as current
        if (_current.Value == null)
        {
            var correlationId = _correlationIdFactory.Create();
            SetCurrent(this);
            LogInitialCorrelationCreated(_logger, correlationId.Id, null);

            // Convert interface to concrete type using the conversion method
            return CertCorrelationId.FromInterface(correlationId);
        }

        // If we have a current context but no correlation ID, create one
        var existingId = _current.Value.CorrelationId;
        if (string.IsNullOrEmpty(existingId))
        {
            var correlationId = _correlationIdFactory.Create();
            LogInitialCorrelationCreated(_logger, correlationId.Id, null);
            return CertCorrelationId.FromInterface(correlationId);
        }

        // Convert the interface to a concrete type
        return CertCorrelationId.FromInterface(_correlationIdFactory.CreateFromExisting(existingId));
    }

    /// <summary>
    /// Adds a property to the correlation context
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    public void AddProperty(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        _properties[key] = value;
        LogPropertyAdded(_logger, key, value, null);
    }

    /// <summary>
    /// Adds multiple properties to the correlation context
    /// </summary>
    /// <param name="properties">Properties to add</param>
    public void AddProperties(IDictionary<string, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        foreach (var kvp in properties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                _properties[kvp.Key] = kvp.Value;
                LogPropertyAdded(_logger, kvp.Key, kvp.Value, null);
            }
        }
    }
}
