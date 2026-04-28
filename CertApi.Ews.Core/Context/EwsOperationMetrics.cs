// CertApi.Ews.Core/Context/EwsOperationMetrics.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Monitoring;
using CertApi.Abstractions.Utilities.Time;
using CertApi.Core.Utilities.Time;
using CertApi.Ews.Core.Interfaces.Context;
using CertApi.Ews.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace CertApi.Ews.Core.Context
{
    /// <summary>
    /// Implementation of EWS operation metrics
    /// </summary>
    public class EwsOperationMetrics : IEwsOperationMetrics
    {
        private readonly ICertOperationMetrics _baseMetrics;
        private readonly ILogger _logger;
        private readonly ICertTimeProvider _timeProvider;
        private readonly ConcurrentDictionary<string, long> _ewsSpecificCounters = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, TimeSpan> _throttlingEvents = new ConcurrentDictionary<string, TimeSpan>();
        private int _throttlingEventCount;
        private long _totalThrottlingTimeMs;
        private int _successfulConnectionCount;
        private int _failedConnectionCount;

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, double, string, Exception?> LogThrottlingEvent =
            LoggerMessage.Define<string, double, string>(
                LogLevel.Warning,
                new EventId(1001, nameof(RecordThrottlingEvent)),
                "EWS throttling event for operation {OperationName} - retry after {RetryAfterMs}ms - CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, bool, double, string, string, Exception?> LogConnectionEvent =
            LoggerMessage.Define<bool, double, string, string>(
                LogLevel.Information,
                new EventId(1002, nameof(RecordConnectionEvent)),
                "EWS connection {Success} to {Endpoint} in {DurationMs}ms - CorrelationId: {CorrelationId}");

        private static readonly Action<ILogger, string, double, bool, Exception?> LogOperationRecorded =
            LoggerMessage.Define<string, double, bool>(
                LogLevel.Debug,
                new EventId(1003, nameof(RecordOperation)),
                "EWS operation {OperationName} recorded. Duration: {DurationMs}ms, Success: {Success}");

        private static readonly Action<ILogger, string, long, Exception?> LogCounterIncremented =
            LoggerMessage.Define<string, long>(
                LogLevel.Debug,
                new EventId(1004, nameof(IncrementCounter)),
                "EWS counter {CounterName} incremented by {Increment}");

        private static readonly Action<ILogger, string, Exception?> LogErrorRecordingThrottlingEvent =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(1005, "ErrorRecordingThrottlingEvent"),
                "Error recording throttling event for operation: {OperationName}");

        private static readonly Action<ILogger, string, Exception?> LogErrorRecordingConnectionEvent =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(1006, "ErrorRecordingConnectionEvent"),
                "Error recording connection event for endpoint: {Endpoint}");

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsOperationMetrics"/> class
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="logger">Logger</param>
        /// <param name="timeProvider">Time provider</param>
        public EwsOperationMetrics(
            string operationId,
            string operationName,
            ILogger logger,
            ICertTimeProvider? timeProvider = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationId);
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? CertTimeProviderFactory.Current;
            _baseMetrics = new CertApi.Core.Context.CertOperationMetrics(operationId, operationName, _timeProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsOperationMetrics"/> class with an existing metrics instance
        /// </summary>
        /// <param name="baseMetrics">Base metrics implementation</param>
        /// <param name="logger">Logger</param>
        /// <param name="timeProvider">Time provider</param>
        public EwsOperationMetrics(
            ICertOperationMetrics baseMetrics,
            ILogger logger,
            ICertTimeProvider? timeProvider = null)
        {
            _baseMetrics = baseMetrics ?? throw new ArgumentNullException(nameof(baseMetrics));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? CertTimeProviderFactory.Current;
        }

        #region ICertOperationMetrics Implementation

        /// <inheritdoc/>
        public long SuccessfulOperations => _baseMetrics.SuccessfulOperations;

        /// <inheritdoc/>
        public long FailedOperations => _baseMetrics.FailedOperations;

        /// <inheritdoc/>
        public long OperationCount => _baseMetrics.OperationCount;

        /// <inheritdoc/>
        public double SuccessRate => _baseMetrics.SuccessRate;

        /// <inheritdoc/>
        public long AverageOperationDurationMs => _baseMetrics.AverageOperationDurationMs;

        /// <inheritdoc/>
        public TimeSpan Duration => _baseMetrics.Duration;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, double> CustomMetrics => _baseMetrics.CustomMetrics;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, long> Counters => _baseMetrics.Counters;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, TimeSpan> Timers => _baseMetrics.Timers;

        #endregion

        #region IEwsOperationMetrics Implementation

        /// <inheritdoc/>
        public int ThrottlingEventCount => _throttlingEventCount;

        /// <inheritdoc/>
        public TimeSpan TotalThrottlingTime => TimeSpan.FromMilliseconds(Interlocked.Read(ref _totalThrottlingTimeMs));

        /// <inheritdoc/>
        public int SuccessfulConnectionCount => _successfulConnectionCount;

        /// <inheritdoc/>
        public int FailedConnectionCount => _failedConnectionCount;

        /// <inheritdoc/>
        public void RecordOperation(string name, TimeSpan duration, bool success)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            // Use prefix for EWS operations if not already present
            string ewsOperationName = name.StartsWith("Ews", StringComparison.OrdinalIgnoreCase)
                ? name
                : $"Ews_{name}";

            _baseMetrics.RecordOperation(ewsOperationName, duration, success);

            // Log EWS specific information using structured logging
            LogOperationRecorded(_logger, name, duration.TotalMilliseconds, success, null);
        }

        /// <inheritdoc/>
        public void RecordCustomMetric(string metricName, double value)
        {
            ArgumentException.ThrowIfNullOrEmpty(metricName);
            _baseMetrics.RecordCustomMetric(metricName, value);
        }

        /// <inheritdoc/>
        public long IncrementCounter(string name, long increment = 1)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            // Add EWS prefix to counter names if not already present
            string ewsCounterName = name.StartsWith("Ews", StringComparison.OrdinalIgnoreCase)
                ? name
                : $"Ews_{name}";

            var result = _baseMetrics.IncrementCounter(ewsCounterName, increment);

            // Log the counter increment
            LogCounterIncremented(_logger, ewsCounterName, increment, null);
            
            return result;
        }

        /// <inheritdoc/>
        public void StartTimer(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            _baseMetrics.StartTimer(name);
        }

        /// <inheritdoc/>
        public TimeSpan StopTimer(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            return _baseMetrics.StopTimer(name);
        }

        /// <inheritdoc/>
        public void RecordTime(string name, TimeSpan elapsedTime)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            _baseMetrics.RecordTime(name, elapsedTime);
        }

        /// <inheritdoc/>
        public void ResetCounters()
        {
            _baseMetrics.ResetCounters();
            _ewsSpecificCounters.Clear();
            _throttlingEvents.Clear();
            Interlocked.Exchange(ref _throttlingEventCount, 0);
            Interlocked.Exchange(ref _totalThrottlingTimeMs, 0);
            Interlocked.Exchange(ref _successfulConnectionCount, 0);
            Interlocked.Exchange(ref _failedConnectionCount, 0);
        }

        /// <inheritdoc/>
        public long GetCounter(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            return _baseMetrics.GetCounter(name);
        }

        /// <inheritdoc/>
        public TimeSpan GetTimerElapsed(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            return _baseMetrics.GetTimerElapsed(name);
        }

        /// <inheritdoc/>
        public IDictionary<string, object> CreateSnapshot()
        {
            var snapshot = _baseMetrics.CreateSnapshot();

            // Add EWS-specific metrics to snapshot
            snapshot["ServiceType"] = "Exchange";
            snapshot["ThrottlingEventCount"] = ThrottlingEventCount;
            snapshot["TotalThrottlingTimeMs"] = TotalThrottlingTime.TotalMilliseconds;
            snapshot["SuccessfulConnectionCount"] = SuccessfulConnectionCount;
            snapshot["FailedConnectionCount"] = FailedConnectionCount;

            // Add any EWS-specific counters
            foreach (var counter in _ewsSpecificCounters)
            {
                snapshot[$"EwsCounter_{counter.Key}"] = counter.Value;
            }

            // Add throttling events
            foreach (var throttlingEvent in _throttlingEvents)
            {
                snapshot[$"EwsThrottling_{throttlingEvent.Key}"] = throttlingEvent.Value.TotalMilliseconds;
            }

            return snapshot;
        }

        /// <inheritdoc/>
        public Task<ICertMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default)
        {
            return _baseMetrics.GetMetricsSnapshotAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void RecordThrottlingEvent(string operationName, TimeSpan retryAfter)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            try
            {
                // Track the throttling event
                _throttlingEvents[operationName] = retryAfter;
                Interlocked.Increment(ref _throttlingEventCount);
                Interlocked.Add(ref _totalThrottlingTimeMs, (long)retryAfter.TotalMilliseconds);

                // Record the counter
                IncrementCounter("ThrottlingEvents");

                // Log the throttling event using the delegate for better performance
                string correlationId = "unknown"; // Try to extract from context if available
                try
                {
                    var operationContext = CertApi.Core.Core.CertCorrelationContext.Current;
                    if (operationContext != null)
                    {
                        correlationId = operationContext.CorrelationId;
                    }
                }
                catch
                {
                    // Ignore if we can't get the correlation context
                }

                LogThrottlingEvent(_logger, operationName, retryAfter.TotalMilliseconds, correlationId, null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Use LoggerMessage delegate for error logging
                LogErrorRecordingThrottlingEvent(_logger, operationName, ex);
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", 
            Justification = "Metrics collection should never throw exceptions that interrupt operations")]
        public void RecordConnectionEvent(bool success, TimeSpan duration, string endpoint)
        {
            ArgumentException.ThrowIfNullOrEmpty(endpoint);

            try
            {
                // Track the connection count
                if (success)
                {
                    Interlocked.Increment(ref _successfulConnectionCount);
                    IncrementCounter("SuccessfulConnections");
                }
                else
                {
                    Interlocked.Increment(ref _failedConnectionCount);
                    IncrementCounter("FailedConnections");
                }

                // Record the connection timing
                RecordTime($"Connection_{endpoint}", duration);

                // Log the connection event using the delegate for better performance
                string correlationId = "unknown"; // Try to extract from context if available
                try
                {
                    var operationContext = CertApi.Core.Core.CertCorrelationContext.Current;
                    if (operationContext != null)
                    {
                        correlationId = operationContext.CorrelationId;
                    }
                }
                catch
                {
                    // Ignore if we can't get the correlation context
                }

                LogConnectionEvent(_logger, success, duration.TotalMilliseconds, endpoint, correlationId, null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Use LoggerMessage delegate for error logging
                LogErrorRecordingConnectionEvent(_logger, endpoint, ex);
            }
        }

        #endregion
    }
}