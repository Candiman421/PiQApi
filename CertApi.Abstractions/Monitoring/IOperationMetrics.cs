// CertApi.Abstractions/Monitoring/IOperationMetrics.cs
namespace CertApi.Abstractions.Monitoring
{
    /// <summary>
    /// Provides metrics tracking for operation execution
    /// </summary>
    public interface IOperationMetrics : IMetricsProvider
    {
        /// <summary>
        /// Gets the number of successful operations
        /// </summary>
        int SuccessfulOperations { get; }

        /// <summary>
        /// Gets the number of failed operations
        /// </summary>
        int FailedOperations { get; }

        /// <summary>
        /// Records an operation execution
        /// </summary>
        void RecordOperation(string operationName, TimeSpan duration, bool success);

        /// <summary>
        /// Records an authentication failure
        /// </summary>
        void RecordAuthenticationFailure(string errorCode);
    }
}