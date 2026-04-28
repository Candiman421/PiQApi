// PiQApi.Core/Core/PiQCorrelationContext.Properties.cs
namespace PiQApi.Core.Core;

public sealed partial class PiQCorrelationContext
{
    /// <summary>
    /// Gets a property value
    /// </summary>
    public T GetProperty<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (_properties.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        throw new KeyNotFoundException($"Property '{key}' not found or not of type {typeof(T).Name}");
    }

    /// <summary>
    /// Tries to get a property value
    /// </summary>
    public bool TryGetProperty<T>(string key, out T? value)
    {
        value = default;

        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (_properties.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the parent correlation identifier
    /// </summary>
    public void SetParentCorrelation(string parentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(parentId);
        ParentCorrelationId = parentId;
        LogParentCorrelationSet(_logger, parentId, null);
    }

    /// <summary>
    /// Checks if a property exists
    /// </summary>
    public bool HasProperty(string key)
    {
        return !string.IsNullOrEmpty(key) && _properties.ContainsKey(key);
    }

    /// <summary>
    /// Sets the current correlation ID
    /// </summary>
    public void SetCorrelationId(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        // Create a new correlation ID and set the current context
        var correlationId = _correlationIdFactory.CreateFromExisting(id);

        // Set this context as current
        _current.Value = this;

        LogCorrelationIdSet(_logger, id, null);
    }
}