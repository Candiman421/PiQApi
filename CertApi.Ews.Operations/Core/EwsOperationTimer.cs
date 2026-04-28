// CertApi.Ews.Operations/Core/EwsOperationTimer.cs
using System.Diagnostics;

namespace CertApi.Ews.Operations.Core
{
    /// <summary>
    /// Implementation of operation timer using Stopwatch
    /// </summary>
    public class EwsOperationTimer : IEwsOperationTimer
    {
        private readonly Stopwatch _stopwatch = new();

        /// <summary>
        /// Gets the elapsed time
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <summary>
        /// Starts or restarts the timer
        /// </summary>
        public void Restart()
        {
            _stopwatch.Restart();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
        }
    }
}