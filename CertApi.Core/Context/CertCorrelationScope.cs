// CertApi.Core/Context/CertCorrelationScope.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Core;
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Context;

/// <summary>
/// Implementation of correlation scope for distributed tracing
/// </summary>
public class CertCorrelationScope : ICertCorrelationScope, IDisposable
{
    private readonly ILogger _logger;
    private readonly IDisposable? _logScope;
    private bool _isDisposed;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, Exception?> LogStartScope =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, "StartScope"),
            "Started correlation scope with ID {CorrelationId}");

    private static readonly Action<ILogger, string, Exception?> LogDisposeScope =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, "DisposeScope"),
            "Disposed correlation scope with ID {CorrelationId}");

    private static readonly Action<ILogger, string, Exception> LogErrorDisposing =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, "ErrorDisposingScope"),
            "Error disposing correlation scope with ID {CorrelationId}");

    /// <summary>
    /// Gets the correlation ID for the scope
    /// </summary>
    public string CorrelationId => Context.CorrelationId;

    /// <summary>
    /// Gets the parent correlation context
    /// </summary>
    public ICertCorrelationContext Context { get; }

    /// <summary>
    /// Gets whether the scope is active
    /// </summary>
    public bool IsActive => !_isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertCorrelationScope"/> class
    /// </summary>
    /// <param name="context">Parent correlation context</param>
    /// <param name="logger">Logger</param>
    public CertCorrelationScope(ICertCorrelationContext context, ILogger logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Create a logging scope with correlation ID
        _logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = Context.CorrelationId
        });

        LogStartScope(_logger, Context.CorrelationId, null);
    }

    /// <summary>
    /// Disposes the scope
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the managed and unmanaged resources
    /// </summary>
    /// <param name="disposing">True to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            try
            {
                // Dispose the logging scope
                _logScope?.Dispose();

                LogDisposeScope(_logger, Context.CorrelationId, null);
            }
            catch (ObjectDisposedException ex)
            {
                // Log but don't throw from Dispose
                LogErrorDisposing(_logger, Context.CorrelationId, ex);
            }
            catch (InvalidOperationException ex)
            {
                // Log but don't throw from Dispose
                LogErrorDisposing(_logger, Context.CorrelationId, ex);
            }
        }

        _isDisposed = true;
    }
}
