// PiQApi.Ews.Core/Models/EwsConnectionMetrics.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PiQApi.Abstractions.Monitoring;

namespace PiQApi.Ews.Core.Models
{
    /// <summary>
    /// Metrics for EWS connections
    /// </summary>
    public class EwsConnectionMetrics : ICertConnectionMetrics, ICertMetricsProvider
    {
        private long _connectionsCreated;
        private long _connectionsAcquired;
        private long _connectionsReleased;
        private long _connectionFailures;
        private long _validationFailures;
        private long _successfulOperations;
        private long _failedOperations;
        private long _totalDurationMs;
        private long _operationCount;
        private readonly ConcurrentDictionary<string, long> _failureReasons = new();
        private DateTime _lastFailureTime = DateTime.MinValue;
        
        // Lock for LastFailureTime updates since DateTime is not atomic
        private readonly object _lastFailureTimeLock = new();

        /// <summary>
        /// Gets the number of connections created
        /// </summary>
        public long ConnectionsCreated => _connectionsCreated;

        /// <summary>
        /// Gets the number of connections acquired
        /// </summary>
        public long ConnectionsAcquired => _connectionsAcquired;

        /// <summary>
        /// Gets the number of connections released
        /// </summary>
        public long ConnectionsReleased => _connectionsReleased;

        /// <summary>
        /// Gets the number of connection failures
        /// </summary>
        public long ConnectionFailures => _connectionFailures;

        /// <summary>
        /// Gets the number of validation failures
        /// </summary>
        public long ValidationFailures => _validationFailures;

        /// <summary>
        /// Gets the failure reasons and counts
        /// </summary>
        public IReadOnlyDictionary<string, long> FailureReasons => _failureReasons;

        /// <summary>
        /// Gets the total number of connections ever acquired
        /// </summary>
        public int TotalConnections => (int)_connectionsAcquired;

        /// <summary>
        /// Gets the number of currently active connections
        /// </summary>
        public int ActiveConnections => (int)Math.Max(0, _connectionsAcquired - _connectionsReleased);

        /// <summary>
        /// Gets the number of connections that have failed
        /// </summary>
        public int FailedConnections => (int)_connectionFailures;

        /// <summary>
        /// Gets the timestamp of the last connection failure
        /// </summary>
        public DateTime LastFailureTime
        {
            get
            {
                lock (_lastFailureTimeLock)
                {
                    return _lastFailureTime;
                }
            }
        }

        /// <summary>
        /// Records a connection created
        /// </summary>
        public void RecordConnectionCreated() => Interlocked.Increment(ref _connectionsCreated);

        /// <summary>
        /// Records a connection acquired
        /// </summary>
        /// <param name="acquisitionTime">Time taken to acquire</param>
        public void RecordConnectionAcquired(TimeSpan acquisitionTime)
        {
            Interlocked.Increment(ref _connectionsAcquired);
            Interlocked.Increment(ref _successfulOperations);
            Interlocked.Increment(ref _operationCount);
            Interlocked.Add(ref _totalDurationMs, (long)acquisitionTime.TotalMilliseconds);
        }

        /// <summary>
        /// Records a connection released
        /// </summary>
        /// <param name="lifetime">Time connection was in use</param>
        public void RecordConnectionReleased(TimeSpan lifetime)
        {
            Interlocked.Increment(ref _connectionsReleased);
            Interlocked.Increment(ref _successfulOperations);
            Interlocked.Increment(ref _operationCount);
            Interlocked.Add(ref _totalDurationMs, (long)lifetime.TotalMilliseconds);
        }

        /// <summary>
        /// Records a connection failure
        /// </summary>
        /// <param name="errorCode">Reason for failure</param>
        public void RecordConnectionFailure(string errorCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(errorCode);

            Interlocked.Increment(ref _connectionFailures);
            Interlocked.Increment(ref _failedOperations);
            Interlocked.Increment(ref _operationCount);
            _failureReasons.AddOrUpdate(errorCode, 1, (_, count) => count + 1);
            
            lock (_lastFailureTimeLock)
            {
                _lastFailureTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Records a validation failure
        /// </summary>
        public void RecordValidationFailure()
        {
            Interlocked.Increment(ref _validationFailures);
            Interlocked.Increment(ref _failedOperations);
            Interlocked.Increment(ref _operationCount);
        }

        /// <summary>
        /// Gets a snapshot of the current metrics asynchronously
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>A task containing the metrics snapshot</returns>
        public Task<ICertMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var failureReasonsSnapshot = new Dictionary<string, long>(_failureReasons);
            
            DateTime lastFailureTimeCopy;
            lock (_lastFailureTimeLock)
            {
                lastFailureTimeCopy = _lastFailureTime;
            }

            var snapshot = new EwsConnectionMetricsSnapshot(
                connectionsCreated: _connectionsCreated,
                connectionsAcquired: _connectionsAcquired,
                connectionsReleased: _connectionsReleased,
                connectionFailures: _connectionFailures,
                totalConnections: TotalConnections,
                activeConnections: ActiveConnections,
                failedConnections: FailedConnections,
                lastFailureTime: lastFailureTimeCopy,
                validationFailures: _validationFailures,
                failureReasons: failureReasonsSnapshot,
                timestamp: DateTimeOffset.UtcNow,
                successfulOperations: _successfulOperations,
                failedOperations: _failedOperations,
                totalDurationMs: _totalDurationMs,
                operationCount: _operationCount);

            return Task.FromResult<ICertMetricsSnapshot>(snapshot);
        }

        /// <summary>
        /// Resets all tracked metrics to their initial state
        /// </summary>
        public void ResetCounters()
        {
            Interlocked.Exchange(ref _connectionsCreated, 0);
            Interlocked.Exchange(ref _connectionsAcquired, 0);
            Interlocked.Exchange(ref _connectionsReleased, 0);
            Interlocked.Exchange(ref _connectionFailures, 0);
            Interlocked.Exchange(ref _validationFailures, 0);
            Interlocked.Exchange(ref _successfulOperations, 0);
            Interlocked.Exchange(ref _failedOperations, 0);
            Interlocked.Exchange(ref _totalDurationMs, 0);
            Interlocked.Exchange(ref _operationCount, 0);
            
            _failureReasons.Clear();
            
            lock (_lastFailureTimeLock)
            {
                _lastFailureTime = DateTime.MinValue;
            }
        }
    }
}