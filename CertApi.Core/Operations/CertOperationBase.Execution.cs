// CertApi.Core/Operations/CertOperationBase.Execution.cs
namespace CertApi.Core.Operations;

public abstract partial class CertOperationBase
{
    /// <summary>
    /// Checks if the operation can be executed
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the operation can be executed; otherwise, false</returns>
    public virtual async Task<bool> IsOperationalAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            return false;
        }

        // Use a scoped operation
        using var scope = CreateLoggingScope();
        LogCheckingOperational(_logger, OperationId, null);

        // Track metrics
        string metricName = "OperationalCheck";
        string correlationMetric = $"{metricName}_{CorrelationId}";
        Context.Metrics.StartTimer(metricName);
        Context.Metrics.IncrementCounter(correlationMetric);

        try
        {
            // Try to initialize if not ready yet
            if (!IsReady)
            {
                try
                {
                    await InitializeAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // CA1031 justification: We're swallowing exceptions during initialization 
                    // and returning false to indicate the operation is not operational.
                    // This method is specifically designed to test operability without throwing.
                    return false;
                }

                // If still not ready after initialization, return false
                if (!IsReady)
                {
                    return false;
                }
            }

            // Check if the operation is in a valid state
            try
            {
                await ValidateStateAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                // CA1031 justification: Similar to above, we're catching all exceptions
                // to determine if the operation is valid without propagating errors.
                // This method is specifically designed to test validity without throwing.
                return false;
            }
        }
        finally
        {
            // Record metrics
            TimeSpan checkTime = Context.Metrics.StopTimer(metricName);
            Context.CorrelationContext.AddProperty("OperationalCheckTimeMs", checkTime.TotalMilliseconds);
            Context.Metrics.RecordTime($"{correlationMetric}_Duration", checkTime);
        }
    }

    /// <summary>
    /// Executes an operation with proper context tracking and error handling
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    protected async Task<T> ExecuteOperationAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        // Ensure the operation is initialized
        if (!IsReady)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        // Create appropriate scopes
        using var loggingScope = CreateLoggingScope(new Dictionary<string, object> { ["OperationName"] = operationName });
        using var operationScope = CreateScope(operationName);

        // Start metrics tracking with correlation ID
        string metricName = operationName;
        string correlationMetric = $"{metricName}_{CorrelationId}";
        Context.Metrics.StartTimer(metricName);
        Context.Metrics.IncrementCounter(correlationMetric);

        try
        {
            // Execute the operation
            var result = await operation(cancellationToken).ConfigureAwait(false);

            // Record success metrics
            Context.Metrics.IncrementCounter($"{metricName}_Success");
            Context.Metrics.IncrementCounter($"{correlationMetric}_Success");

            return result;
        }
        catch (Exception ex)
        {
            // Record failure metrics
            Context.Metrics.IncrementCounter($"{metricName}_Failure");
            Context.Metrics.IncrementCounter($"{correlationMetric}_Failure");

            // Add correlation ID to exception data
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationId))
            {
                ex.Data["CorrelationId"] = CorrelationId;
                ex.Data["OperationId"] = OperationId;
                ex.Data["OperationName"] = operationName;
            }

            // Log the error
            await LogErrorAsync(ex, new Dictionary<string, object> { ["OperationName"] = operationName }).ConfigureAwait(false);

            throw;
        }
        finally
        {
            // Record execution time
            TimeSpan executionTime = Context.Metrics.StopTimer(metricName);
            Context.CorrelationContext.AddProperty($"{metricName}TimeMs", executionTime.TotalMilliseconds);
            Context.Metrics.RecordTime($"{correlationMetric}_Duration", executionTime);
        }
    }

    /// <summary>
    /// Executes an operation with proper context tracking and error handling
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="operationName">Name of the operation for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected async Task ExecuteOperationAsync(
        Func<CancellationToken, Task> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        // Ensure the operation is initialized
        if (!IsReady)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        // Create appropriate scopes
        using var loggingScope = CreateLoggingScope(new Dictionary<string, object> { ["OperationName"] = operationName });
        using var operationScope = CreateScope(operationName);

        // Start metrics tracking with correlation ID
        string metricName = operationName;
        string correlationMetric = $"{metricName}_{CorrelationId}";
        Context.Metrics.StartTimer(metricName);
        Context.Metrics.IncrementCounter(correlationMetric);

        try
        {
            // Execute the operation
            await operation(cancellationToken).ConfigureAwait(false);

            // Record success metrics
            Context.Metrics.IncrementCounter($"{metricName}_Success");
            Context.Metrics.IncrementCounter($"{correlationMetric}_Success");
        }
        catch (Exception ex)
        {
            // Record failure metrics
            Context.Metrics.IncrementCounter($"{metricName}_Failure");
            Context.Metrics.IncrementCounter($"{correlationMetric}_Failure");

            // Add correlation ID to exception data
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationId))
            {
                ex.Data["CorrelationId"] = CorrelationId;
                ex.Data["OperationId"] = OperationId;
                ex.Data["OperationName"] = operationName;
            }

            // Log the error
            await LogErrorAsync(ex, new Dictionary<string, object> { ["OperationName"] = operationName }).ConfigureAwait(false);

            throw;
        }
        finally
        {
            // Record execution time
            TimeSpan executionTime = Context.Metrics.StopTimer(metricName);
            Context.CorrelationContext.AddProperty($"{metricName}TimeMs", executionTime.TotalMilliseconds);
            Context.Metrics.RecordTime($"{correlationMetric}_Duration", executionTime);
        }
    }
}