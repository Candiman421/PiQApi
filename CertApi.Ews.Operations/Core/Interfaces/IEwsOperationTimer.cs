// CertApi.Ews.Operations/Core/Interfaces/IEwsOperationTimer.cs
namespace CertApi.Ews.Operations.Core.Interfaces
{
    /// <summary>
    /// Interface for operation timing
    /// </summary>
    public interface IEwsOperationTimer
    {
        /// <summary>
        /// Starts or restarts the timer
        /// </summary>
        void Restart();

        /// <summary>
        /// Stops the timer
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the elapsed time
        /// </summary>
        TimeSpan Elapsed { get; }
    }
}