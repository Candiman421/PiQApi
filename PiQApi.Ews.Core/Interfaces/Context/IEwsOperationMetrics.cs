// PiQApi.Ews.Core/Interfaces/Context/IEwsOperationMetrics.cs

using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Monitoring;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PiQApi.Ews.Core.Interfaces.Context
{
    /// <summary>
    /// Interface for Exchange Web Services operation metrics
    /// Extends the core operation metrics interface with EWS-specific functionality
    /// </summary>
    public interface IEwsOperationMetrics : IPiQOperationMetrics
    {
        /// <summary>
        /// Gets the count of throttling events that have occurred
        /// </summary>
        int ThrottlingEventCount { get; }

        /// <summary>
        /// Gets the total time spent in throttled state
        /// </summary>
        TimeSpan TotalThrottlingTime { get; }

        /// <summary>
        /// Gets the total count of successful EWS connections
        /// </summary>
        int SuccessfulConnectionCount { get; }

        /// <summary>
        /// Gets the total count of failed EWS connections
        /// </summary>
        int FailedConnectionCount { get; }

        /// <summary>
        /// Records an EWS operation with its duration and success status
        /// </summary>
        /// <param name="name">Name of the operation</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="success">Whether the operation was successful</param>
        new void RecordOperation(string name, TimeSpan duration, bool success);

        /// <summary>
        /// Increments an EWS-specific counter by the specified amount
        /// </summary>
        /// <param name="name">Counter name</param>
        /// <param name="increment">The amount to increment by</param>
        /// <returns>The new counter value</returns>
        new long IncrementCounter(string name, long increment = 1);

        /// <summary>
        /// Records an EWS-specific throttling event
        /// </summary>
        /// <param name="operationName">Name of the throttled operation</param>
        /// <param name="retryAfter">Time to wait before retrying</param>
        void RecordThrottlingEvent(string operationName, TimeSpan retryAfter);

        /// <summary>
        /// Records an EWS connection event
        /// </summary>
        /// <param name="success">Whether the connection was successful</param>
        /// <param name="duration">Duration of the connection attempt</param>
        /// <param name="endpoint">The endpoint connected to</param>
        void RecordConnectionEvent(bool success, TimeSpan duration, string endpoint);

        /// <summary>
        /// Creates a dictionary with EWS-specific metrics
        /// </summary>
        /// <returns>Dictionary containing EWS-specific metrics</returns>
        new IDictionary<string, object> CreateSnapshot();

        /// <summary>
        /// Gets EWS-specific metrics snapshot that includes throttling and connection metrics
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Metrics snapshot with EWS-specific metrics</returns>
        new Task<IPiQMetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets all EWS-specific counters
        /// </summary>
        new void ResetCounters();
    }
}