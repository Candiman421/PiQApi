// PiQApi.Core/Factories/PiQCorrelationIdFactory.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Core.Models;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Factories;

/// <summary>
/// Factory for creating correlation identifiers
/// </summary>
public sealed class PiQCorrelationIdFactory : IPiQCorrelationIdFactory
{
    private readonly ILogger<PiQCorrelationIdFactory> _logger;
    private readonly IPiQTimeProvider _timeProvider;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, Exception?> LogCreatedCorrelationId =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, "Create"),
            "Created new correlation ID: {PiQCorrelationId}");

    private static readonly Action<ILogger, string, Exception?> LogCreatedFromExisting =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, "CreateFromExisting"),
            "Created correlation ID from existing: {PiQCorrelationId}");

    /// <summary>
    /// Creates a new instance of the correlation ID factory
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    public PiQCorrelationIdFactory(
        ILogger<PiQCorrelationIdFactory> logger,
        IPiQTimeProvider? timeProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? PiQTimeProviderFactory.Current; // Use static property
    }

    /// <inheritdoc />
    public IPiQCorrelationId Create()
    {
        var correlationId = new PiQCorrelationId(Guid.NewGuid().ToString(), _timeProvider);
        LogCreatedCorrelationId(_logger, correlationId.Id, null);
        return correlationId;
    }

    /// <inheritdoc />
    public IPiQCorrelationId CreateFromExisting(string existingId)
    {
        ArgumentException.ThrowIfNullOrEmpty(existingId);

        var correlationId = new PiQCorrelationId(existingId, _timeProvider);
        LogCreatedFromExisting(_logger, correlationId.Id, null);
        return correlationId;
    }
}