// PiQApi.Core/Validation/PiQValidationContextFactory.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Abstractions.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace PiQApi.Core.Validation;

/// <summary>
/// Factory for creating validation contexts
/// </summary>
public class PiQValidationContextFactory : IPiQValidationContextFactory
{
    private readonly IPiQCorrelationIdFactory _correlationIdFactory;
    private readonly ILogger<PiQValidationContextFactory> _logger;
    private readonly IPiQTimeProvider _timeProvider;

    #region LoggerMessage Delegates

    private static readonly Action<ILogger, string, string, Exception?> LogCreatingContext =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, "CreateForService"),
            "Creating validation context with correlation ID {CorrelationId} and service version {ServiceVersion}");

    private static readonly Action<ILogger, string, Exception?> LogGeneratingId =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, "CreateForService"),
            "Generating new correlation ID for validation context with service version {ServiceVersion}");

    private static readonly Action<ILogger, string, string, Exception?> LogAddingVersion =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(3, "AddServiceVersion"),
            "Adding service version {ServiceVersion} to validation context with correlation ID {CorrelationId}");

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQValidationContextFactory"/> class
    /// </summary>
    /// <param name="correlationIdFactory">Factory for creating correlation IDs</param>
    /// <param name="logger">Logger</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public PiQValidationContextFactory(
        IPiQCorrelationIdFactory correlationIdFactory,
        ILogger<PiQValidationContextFactory> logger,
        IPiQTimeProvider timeProvider)
    {
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Creates a validation context for a service
    /// </summary>
    /// <param name="correlationId">The correlation ID for tracing</param>
    /// <param name="serviceVersion">The service version to use</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A configured validation context</returns>
    public IPiQValidationContext CreateForService(
        string correlationId,
        string serviceVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId, nameof(correlationId));
        ArgumentException.ThrowIfNullOrEmpty(serviceVersion, nameof(serviceVersion));

        LogCreatingContext(_logger, correlationId, serviceVersion, null);

        // Create a basic validation context
        var baseContext = new PiQValidationContext(
            Abstractions.Enums.ValidationModeType.Standard, // Default mode
            0, // Initial depth
            10, // Default max depth
            false, // Default for aggregating errors
            correlationId, // Use the provided correlation ID directly
            ImmutableDictionary<string, object>.Empty, // Start with empty context
            _timeProvider,
            cancellationToken);

        // Add service version
        return AddServiceVersion(baseContext, serviceVersion);
    }

    /// <summary>
    /// Creates a validation context with a generated correlation ID
    /// </summary>
    /// <param name="serviceVersion">The service version to use</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A configured validation context</returns>
    public IPiQValidationContext CreateForService(
        string serviceVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceVersion, nameof(serviceVersion));

        LogGeneratingId(_logger, serviceVersion, null);

        // Create a new correlation ID
        IPiQCorrelationId certCorrelationId = _correlationIdFactory.Create();

        // Create basic validation context with the new correlation ID
        var baseContext = new PiQValidationContext(
            Abstractions.Enums.ValidationModeType.Standard, // Default mode
            0, // Initial depth
            10, // Default max depth
            false, // Default for aggregating errors
            certCorrelationId.Id, // Use the ID string from correlation ID
            ImmutableDictionary<string, object>.Empty, // Start with empty context
            _timeProvider,
            cancellationToken);

        // Add service version
        return AddServiceVersion(baseContext, serviceVersion);
    }

    /// <summary>
    /// Adds service version to an existing context
    /// </summary>
    /// <param name="context">The existing validation context</param>
    /// <param name="serviceVersion">The service version to add</param>
    /// <returns>The updated validation context</returns>
    public IPiQValidationContext AddServiceVersion(
        IPiQValidationContext context,
        string serviceVersion)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentException.ThrowIfNullOrEmpty(serviceVersion, nameof(serviceVersion));

        LogAddingVersion(_logger, serviceVersion, context.CorrelationId, null);

        // Add the service version as a context value
        return context.WithContextValue("ServiceVersion", serviceVersion);
    }
}