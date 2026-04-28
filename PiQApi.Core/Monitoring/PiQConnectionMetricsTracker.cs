// PiQApi.Core/Monitoring/PiQConnectionMetricsTracker.cs
using PiQApi.Abstractions.Monitoring;
using PiQApi.Abstractions.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Monitoring;

/// <summary>
/// Tracks connection metrics in a thread-safe manner
/// </summary>
public class PiQConnectionMetricsTracker : IPiQConnectionMetrics
{
    private readonly ILogger<PiQConnectionMetricsTracker> _logger;
    private readonly IPiQTimeProvider _timeProvider;
    private int _totalConnections;
    private int _activeConnections;
    private int _failedConnections;
    private DateTime _lastFailureTime = DateTime.MinValue;

    private static readonly Action<ILogger, int, int, int, Exception?> LogConnectionState =
        LoggerMessage.Define<int, int, int>(
            LogLevel.Debug,
            new EventId(1, "ConnectionState"),
            "Connection state updated - Total: {Total}, Active: {Active}, Failed: {Failed}");

    private static readonly Action<ILogger, string, Exception?> LogConnectionFailure =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, "ConnectionFailure"),
            "Connection failure recorded: {ErrorCode}");

    /// <summary>
    /// Initializes a new instance of the PiQConnectionMetricsTracker class
    /// </summary>
    /// <param name="logger">Logger for recording metrics events</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public PiQConnectionMetricsTracker(
        ILogger<PiQConnectionMetricsTracker> logger,
        IPiQTimeProvider timeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public int TotalConnections => Interlocked.CompareExchange(ref _totalConnections, 0, 0);

    /// <inheritdoc />
    public int ActiveConnections => Interlocked.CompareExchange(ref _activeConnections, 0, 0);

    /// <inheritdoc />
    public int FailedConnections => Interlocked.CompareExchange(ref _failedConnections, 0, 0);

    /// <inheritdoc />
    public DateTime LastFailureTime => _lastFailureTime;

    /// <inheritdoc />
    public void RecordConnectionAcquired(TimeSpan acquisitionTime)
    {
        var newTotal = Interlocked.Increment(ref _totalConnections);
        var newActive = Interlocked.Increment(ref _activeConnections);

        LogConnectionState(_logger, newTotal, newActive, FailedConnections, null);
    }

    /// <inheritdoc />
    public void RecordConnectionReleased(TimeSpan lifetime)
    {
        int currentActive, newActive;
        do
        {
            currentActive = Interlocked.CompareExchange(ref _activeConnections, 0, 0);
            if (currentActive <= 0)
            {
                newActive = 0;
                break;
            }

            newActive = currentActive - 1;
        } while (Interlocked.CompareExchange(ref _activeConnections, newActive, currentActive) != currentActive);

        LogConnectionState(_logger, TotalConnections, newActive, FailedConnections, null);
    }

    /// <inheritdoc />
    public void RecordConnectionFailure(string errorCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        // Decrement active connections
        int currentActive, newActive;
        do
        {
            currentActive = Interlocked.CompareExchange(ref _activeConnections, 0, 0);
            if (currentActive <= 0)
            {
                newActive = 0;
                break;
            }

            newActive = currentActive - 1;
        } while (Interlocked.CompareExchange(ref _activeConnections, newActive, currentActive) != currentActive);

        // Increment failed connections
        Interlocked.Increment(ref _failedConnections);

        // Record failure time
        _lastFailureTime = _timeProvider.UtcNow;

        LogConnectionState(_logger, TotalConnections, newActive, FailedConnections, null);
        LogConnectionFailure(_logger, errorCode, null);
    }

    /// <inheritdoc />
    public void ResetCounters()
    {
        Interlocked.Exchange(ref _totalConnections, 0);
        Interlocked.Exchange(ref _activeConnections, 0);
        Interlocked.Exchange(ref _failedConnections, 0);
        _lastFailureTime = DateTime.MinValue;
    }

    /// <inheritdoc />
    public async Task<IPiQMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // Check for cancellation
        cancellationToken.ThrowIfCancellationRequested();

        // Read all counters atomically
        var total = Interlocked.CompareExchange(ref _totalConnections, 0, 0);
        var active = Interlocked.CompareExchange(ref _activeConnections, 0, 0);
        var failed = Interlocked.CompareExchange(ref _failedConnections, 0, 0);

        // Create snapshot with connection metrics
        var snapshot = new PiQMetricsSnapshot(
            totalConnections: total,
            activeConnections: active,
            failedConnections: failed
        );

        return await Task.FromResult<IPiQMetricsSnapshot>(snapshot).ConfigureAwait(false);
    }
}