// CertApi.Core/Core/CertCorrelationScopeDisposer.cs
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Core;

/// <summary>
/// Disposable wrapper for scope cleanup
/// </summary>
internal sealed class CertCorrelationScopeDisposer : IDisposable
{
    private readonly CertCorrelationContext _context;
    private readonly CertCorrelationScopeState _scopeState;
    private readonly ILogger _logger;
    private bool _disposed;

    // Logger message delegates for better performance
    private static readonly Action<ILogger, string, Exception?> LogParentRestored =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(6, "ScopeDisposed"),
            "Restored parent correlation ID: {ParentId}");

    private static readonly Action<ILogger, Exception?> LogCorrelationCleared =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(7, "ScopeDisposed"),
            "Cleared correlation ID at scope end");

    private static readonly Action<ILogger, Exception> LogDisposalError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(8, "DisposalError"),
            "Error in correlation scope disposal");

    /// <summary>
    /// Creates a new scope disposer
    /// </summary>
    /// <param name="context">The correlation context</param>
    /// <param name="scopeState">The scope state</param>
    /// <param name="logger">The logger</param>
    public CertCorrelationScopeDisposer(
        CertCorrelationContext context,
        CertCorrelationScopeState scopeState,
        ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _scopeState = scopeState ?? throw new ArgumentNullException(nameof(scopeState));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Disposes the scope
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            // Let the context handle the scope ending
            _context.EndInternalScope();

            // Log appropriate message
            if (_scopeState.ParentId != null)
            {
                LogParentRestored(_logger, _scopeState.ParentId, null);
            }
            else
            {
                LogCorrelationCleared(_logger, null);
            }
        }
        // Note: CA1031 - Broad exception catching is appropriate in Dispose methods
        // to prevent exceptions from propagating to callers
        catch (Exception ex)
        {
            // Log but don't throw from Dispose
            LogDisposalError(_logger, ex);
        }
    }
}