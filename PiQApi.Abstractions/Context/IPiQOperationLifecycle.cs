// PiQApi.Abstractions/Context/IPiQOperationLifecycle.cs
namespace PiQApi.Abstractions.Context;

/// <summary>
/// Defines lifecycle operations for an operation context
/// </summary>
public interface IPiQOperationLifecycle
{
    /// <summary>
    /// Gets the cancellation token for this operation
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Initializes the operation context
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the operation context
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ValidateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a child operation context with the specified name
    /// </summary>
    /// <param name="operationName">Name of the child operation</param>
    /// <returns>A new operation context with this context as parent</returns>
    IPiQOperationContext CreateChildContext(string operationName);

    /// <summary>
    /// Creates a scope for the current operation
    /// </summary>
    /// <returns>A disposable scope</returns>
    IDisposable CreateScope();

    /// <summary>
    /// Creates a named scope for the current operation
    /// </summary>
    /// <param name="scopeName">Name of the scope</param>
    /// <returns>A disposable scope that will record metrics when disposed</returns>
    IDisposable CreateScope(string scopeName);
}