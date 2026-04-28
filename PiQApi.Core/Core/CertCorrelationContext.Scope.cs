// PiQApi.Core/Core/CertCorrelationContext.Scope.cs
namespace PiQApi.Core.Core;

public sealed partial class CertCorrelationContext
{
    /// <summary>
    /// Creates a new correlation scope with a new ID
    /// </summary>
    public IDisposable CreateScope()
    {
        var parentId = GetOrCreateCorrelationId().Id;
        var newId = _correlationIdFactory.Create();

        // Create a scope state
        var scopeState = new CertCorrelationScopeState(parentId);
        _scopeStack.Push(scopeState);

        // Set the parent correlation ID
        if (!string.IsNullOrEmpty(parentId))
        {
            SetParentCorrelation(parentId);
        }

        // Set this context as current
        _current.Value = this;

        return new CertCorrelationScopeDisposer(this, scopeState, _logger);
    }

    /// <summary>
    /// Creates a new correlation scope with a specific ID
    /// </summary>
    public IDisposable CreateScope(string correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId);

        var parentId = GetOrCreateCorrelationId().Id;
        var newId = _correlationIdFactory.CreateFromExisting(correlationId);

        // Create a scope state
        var scopeState = new CertCorrelationScopeState(parentId);
        _scopeStack.Push(scopeState);

        // Set the parent correlation ID
        if (!string.IsNullOrEmpty(parentId))
        {
            SetParentCorrelation(parentId);
        }

        // Set this context as current
        _current.Value = this;

        return new CertCorrelationScopeDisposer(this, scopeState, _logger);
    }

    /// <summary>
    /// Creates an internal scope for the context
    /// </summary>
    /// <remarks>
    /// This method is used by CertCorrelationScope to manage scope without static methods
    /// </remarks>
    internal void CreateInternalScope()
    {
        var parentId = GetOrCreateCorrelationId().Id;

        // Create a scope state
        var scopeState = new CertCorrelationScopeState(parentId);
        _scopeStack.Push(scopeState);

        // Set this context as current
        _current.Value = this;
    }

    /// <summary>
    /// Ends an internal scope for the context
    /// </summary>
    /// <remarks>
    /// This method is used by CertCorrelationScope to manage scope without static methods
    /// </remarks>
    internal void EndInternalScope()
    {
        if (_scopeStack.Count > 0)
        {
            var scopeState = _scopeStack.Pop();

            // Restore parent context if available
            if (scopeState.ParentId != null)
            {
                // If parent ID is available, set it
                _ = _correlationIdFactory.CreateFromExisting(scopeState.ParentId);
            }
            else
            {
                // Clear current context if no parent
                _current.Value = null;
            }
        }
    }
}