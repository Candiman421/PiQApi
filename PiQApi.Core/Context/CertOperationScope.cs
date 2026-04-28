// PiQApi.Core/Context/CertOperationScope.cs
using PiQApi.Abstractions.Context;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Context;

/// <summary>
/// Implementation of operation scope for timing and metrics
/// </summary>
public class CertOperationScope : ICertOperationScope, IDisposable
{
    private readonly ILogger _logger;
    private readonly ICertOperationMetrics _metrics;
    private readonly DateTimeOffset _startTime;
    private bool _isDisposed;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, string, Exception?> LogScopeStart =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, "ScopeStart"),
            "Started scope {ScopeName} for operation {OperationId}");

    private static readonly Action<ILogger, string, string, double, Exception?> LogScopeComplete =
        LoggerMessage.Define<string, string, double>(
            LogLevel.Debug,
            new EventId(2, "ScopeComplete"),
            "Completed scope {ScopeName} for operation {OperationId} in {DurationMs}ms");

    private static readonly Action<ILogger, string, string, Exception> LogScopeError =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(3, "ScopeError"),
            "Error disposing scope {ScopeName} for operation {OperationId}");

    /// <summary>
    /// Gets the name of the scope
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the parent operation context
    /// </summary>
    public ICertOperationContext Context { get; }

    /// <summary>
    /// Gets whether the scope is active
    /// </summary>
    public bool IsActive => !_isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertOperationScope"/> class
    /// </summary>
    /// <param name="scopeName">Name of the scope</param>
    /// <param name="context">Parent operation context</param>
    /// <param name="logger">Logger</param>
    /// <param name="metrics">Operation metrics</param>
    public CertOperationScope(
        string scopeName,
        ICertOperationContext context,
        ILogger logger,
        ICertOperationMetrics metrics)
    {
        Name = scopeName ?? throw new ArgumentNullException(nameof(scopeName));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _startTime = DateTimeOffset.UtcNow;

        // Start timing the scope
        _metrics.StartTimer(Name);

        // Log scope creation
        LogScopeStart(_logger, Name, Context.Identifier.Id, null);
    }

    /// <summary>
    /// Disposes the scope and records metrics
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
                // Stop timing and record duration from metrics system
                var duration = _metrics.StopTimer(Name);

                // Calculate duration from local timestamp as a verification
                var calculatedDuration = DateTimeOffset.UtcNow - _startTime;

                // Add both durations to the context properties for comparison if needed
                Context.AddProperty($"Scope_{Name}_DurationMs", duration.TotalMilliseconds);
                Context.AddProperty($"Scope_{Name}_CalculatedDurationMs", calculatedDuration.TotalMilliseconds);

                // Log scope completion
                LogScopeComplete(_logger, Name, Context.Identifier.Id, duration.TotalMilliseconds, null);
            }
            // Note: CA1031 - Catching general Exception is appropriate in Dispose methods
            // to prevent exceptions from propagating to callers
            catch (Exception ex)
            {
                // Log but don't throw from Dispose
                LogScopeError(_logger, Name, Context.Identifier.Id, ex);
            }
        }

        _isDisposed = true;
    }
}