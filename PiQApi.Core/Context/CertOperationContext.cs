// PiQApi.Core/Context/CertOperationContext.cs
using System.Collections.Concurrent;
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Context;

/// <summary>
/// Implementation of the operation context interface that provides
/// contextual information for service operations.
/// </summary>
public partial class CertOperationContext : ICertOperationContext, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, object> _properties = new();
    private bool _isDisposed;
    private bool _asyncLockDisposed;
    private readonly SemaphoreSlim _asyncLock = new(1, 1);
    private readonly ILogger _logger;
    private readonly ICertOperationValidator _validator;
    private readonly ICertOperationResources _resources;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertOperationContext"/> class
    /// </summary>
    /// <param name="identifier">Operation identifier</param>
    /// <param name="state">Operation state</param>
    /// <param name="metrics">Operation metrics</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <param name="validator">Operation validator</param>
    /// <param name="resources">Operation resources</param>
    /// <param name="logger">Logger</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public CertOperationContext(
        ICertOperationIdentifier identifier,
        ICertOperationState state,
        ICertOperationMetrics metrics,
        ICertCorrelationContext correlationContext,
        ICertOperationValidator validator,
        ICertOperationResources resources,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        State = state ?? throw new ArgumentNullException(nameof(state));
        Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        CorrelationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CancellationToken = cancellationToken;

        // Initialize with basic properties
        _properties["StartTime"] = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the operation identifier information
    /// </summary>
    public ICertOperationIdentifier Identifier { get; }

    /// <summary>
    /// Gets the operation state information
    /// </summary>
    public ICertOperationState State { get; }

    /// <summary>
    /// Gets the operation metrics information
    /// </summary>
    public ICertOperationMetrics Metrics { get; }

    /// <summary>
    /// Gets the underlying correlation context
    /// </summary>
    public ICertCorrelationContext CorrelationContext { get; }

    /// <summary>
    /// Gets the cancellation token for this operation
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets a collection of properties specific to this operation
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Helper property to get the operation ID from identifier
    /// </summary>
    public string OperationId => Identifier?.Id ?? string.Empty;

    /// <summary>
    /// Helper property to get the correlation ID from context
    /// </summary>
    public string CorrelationId => CorrelationContext?.CorrelationId ?? string.Empty;

    /// <summary>
    /// Helper property to get the operation status from state
    /// </summary>
    public OperationStatusType Status => State?.Status ?? OperationStatusType.Created;

    /// <summary>
    /// Adds a property to the operation
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    public void AddProperty(string key, object value)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        _properties[key] = value;

        // Also add to correlation context for cross-operation visibility
        if (!CorrelationContext.HasProperty(key))
        {
            CorrelationContext.AddProperty(key, value);
        }
    }

    /// <summary>
    /// Adds multiple properties to the operation
    /// </summary>
    /// <param name="properties">Properties to add</param>
    public void AddProperties(IDictionary<string, object> properties)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(properties);

        foreach (var kvp in properties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                _properties[kvp.Key] = kvp.Value;

                // Also add to correlation context for cross-operation visibility
                if (!CorrelationContext.HasProperty(kvp.Key))
                {
                    CorrelationContext.AddProperty(kvp.Key, kvp.Value);
                }
            }
        }
    }

    /// <summary>
    /// Gets a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <returns>Property value</returns>
    public T GetPropertyValue<T>(string key)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (_properties.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        throw new KeyNotFoundException($"Property '{key}' of type '{typeof(T).Name}' not found");
    }

    /// <summary>
    /// Tries to get a property value
    /// </summary>
    /// <typeparam name="T">Type of property</typeparam>
    /// <param name="key">Property key</param>
    /// <param name="value">Output property value</param>
    /// <returns>True if property exists and is of correct type</returns>
    public bool TryGetPropertyValue<T>(string key, out T? value)
    {
        value = default;

        if (string.IsNullOrEmpty(key) || _isDisposed || !_properties.TryGetValue(key, out var propertyValue))
        {
            return false;
        }

        if (propertyValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        try
        {
            value = (T)Convert.ChangeType(propertyValue, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a property exists
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>True if property exists</returns>
    public bool HasProperty(string key)
    {
        ThrowIfDisposed();
        return !string.IsNullOrEmpty(key) && _properties.ContainsKey(key);
    }

    /// <summary>
    /// Creates a scope for the current operation (interface implementation without parameters)
    /// </summary>
    /// <returns>A disposable scope</returns>
    public IDisposable CreateScope()
    {
        ThrowIfDisposed();

        const string defaultScopeName = "DefaultScope";

        // Start timing the scope
        Metrics.StartTimer(defaultScopeName);

        // Log scope creation
        LogScopeStarted(_logger, defaultScopeName, Identifier.Id, null);

        // Create a new scope with the current context - direct implementation avoids null warning
        return new CertOperationScope(defaultScopeName, this, _logger, Metrics);
    }

    /// <summary>
    /// Disposes the operation context asynchronously
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            // Only attempt to acquire lock if it's not already disposed
            if (!_asyncLockDisposed)
            {
                await _asyncLock.WaitAsync().ConfigureAwait(false);
            }
        }
        catch (ObjectDisposedException)
        {
            // SemaphoreSlim was already disposed, which is fine at this point
            return;
        }

        try
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            // If operation is still in progress, log completion
            if (State.IsInProgress)
            {
                try
                {
                    await LogOperationEndAsync(true).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogOperationEndError(_logger, ex, Identifier.Id, null);
                }
            }
        }
        finally
        {
            // Mark semaphore as disposed and dispose it only once
            if (!_asyncLockDisposed)
            {
                _asyncLockDisposed = true;

                // Release first, then dispose
                try
                {
                    _asyncLock.Release();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore - it might already be disposed
                }
                catch (SemaphoreFullException)
                {
                    // Ignore - the semaphore was already released
                }

                _asyncLock.Dispose();
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Checks if the context is disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the context is disposed</exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, nameof(CertOperationContext));
    }
}
