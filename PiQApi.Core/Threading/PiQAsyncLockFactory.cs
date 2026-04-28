// PiQApi.Core/Threading/PiQAsyncLockFactory.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Threading;

/// <summary>
/// Factory for creating asynchronous locks
/// </summary>
public sealed class PiQAsyncLockFactory : IPiQAsyncLockFactory
{
    private readonly ILogger<PiQAsyncLock>? _logger;
    private readonly IPiQCorrelationContext? _correlationContext;
    private readonly IPiQOperationMetrics? _metrics;

    #region LoggerMessage Delegates

    private static readonly Action<ILogger, string, Exception?> LogCreatingAsyncLock =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, "Create"),
            "Creating async lock with ID {LockId}");

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQAsyncLockFactory"/> class
    /// </summary>
    public PiQAsyncLockFactory() : this(null, null, null)
    {
        // Default constructor with no dependencies for simple scenarios
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQAsyncLockFactory"/> class
    /// </summary>
    /// <param name="logger">Logger for lock operations</param>
    /// <param name="correlationContext">Correlation context for distributed tracing</param>
    /// <param name="metrics">Metrics for tracking lock operations</param>
    public PiQAsyncLockFactory(
        ILogger<PiQAsyncLock>? logger = null,
        IPiQCorrelationContext? correlationContext = null,
        IPiQOperationMetrics? metrics = null)
    {
        _logger = logger;
        _correlationContext = correlationContext;
        _metrics = metrics;
    }

    /// <summary>
    /// Creates a new asynchronous lock
    /// </summary>
    /// <returns>A new asynchronous lock instance</returns>
    public IPiQAsyncLock Create()
    {
        // Track lock creation in correlation context if available
        _correlationContext?.AddProperty("ThreadingOperation", "AsyncLockCreation");

        var lockId = Guid.NewGuid().ToString();
        _correlationContext?.AddProperty("LockId", lockId);

        if (_logger != null)
        {
            LogCreatingAsyncLock(_logger, lockId, null);
        }
        _metrics?.IncrementCounter("async_locks_created");

        return new PiQAsyncLock(lockId, _logger);
    }
}
