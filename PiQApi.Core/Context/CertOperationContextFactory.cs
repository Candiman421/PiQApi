// PiQApi.Core/Context/CertOperationContextFactory.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Core;
using PiQApi.Core.Factories;
using PiQApi.Core.Validation;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Context;

/// <summary>
/// Factory for creating operation context instances
/// </summary>
public class CertOperationContextFactory : ICertOperationContextFactory
{
    private readonly ICertCorrelationIdFactory _correlationIdFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ICertExceptionFactory _exceptionFactory;
    private readonly ICertValidationResultFactory _validationResultFactory;

    #region LoggerMessage Delegates

    // High-performance logging delegates
    private static readonly Action<ILogger, string, string, Exception?> LogCreatedContext =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(Create)),
            "Created operation context with name {OperationName} and correlation ID {CorrelationId}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogCreatedChildContext =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Debug,
            new EventId(2, nameof(CreateChild)),
            "Created child operation context {ChildName} with parent {ParentId} and correlation ID {CorrelationId}");

    #endregion

    /// <summary>
    /// Creates a new instance of the operation context factory
    /// </summary>
    /// <param name="correlationIdFactory">Correlation ID factory</param>
    /// <param name="loggerFactory">Logger factory</param>
    /// <param name="exceptionFactory">Exception factory</param>
    /// <param name="validationResultFactory">Validation result factory</param>
    public CertOperationContextFactory(
        ICertCorrelationIdFactory correlationIdFactory,
        ILoggerFactory loggerFactory,
        ICertExceptionFactory exceptionFactory,
        ICertValidationResultFactory validationResultFactory)
    {
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        _validationResultFactory = validationResultFactory ?? throw new ArgumentNullException(nameof(validationResultFactory));
    }

    /// <summary>
    /// Creates a context with the specified operation name and correlation context
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <returns>Operation context</returns>
    public ICertOperationContext Create(string operationName, ICertCorrelationContext correlationContext)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));
        ArgumentNullException.ThrowIfNull(correlationContext, nameof(correlationContext));

        var context = CreateConfigured(
            operationName,
            correlationContext,
            OperationType.Generic);

        var logger = _loggerFactory.CreateLogger<CertOperationContextFactory>();
        LogCreatedContext(logger, operationName, correlationContext.CorrelationId, null);

        return context;
    }

    /// <summary>
    /// Creates a context with the specified properties
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <param name="properties">Initial properties</param>
    /// <returns>Operation context</returns>
    public ICertOperationContext CreateWithProperties(
        string operationName,
        ICertCorrelationContext correlationContext,
        IDictionary<string, object> properties)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));
        ArgumentNullException.ThrowIfNull(correlationContext, nameof(correlationContext));
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));

        var context = Create(operationName, correlationContext);

        foreach (var kvp in properties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value is not null)
            {
                context.AddProperty(kvp.Key, kvp.Value);
            }
        }

        return context;
    }

    /// <summary>
    /// Creates a fully configured context
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <param name="operationType">Type of operation</param>
    /// <param name="timeout">Operation timeout</param>
    /// <param name="parent">Parent context, if any</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation context</returns>
    public ICertOperationContext CreateConfigured(
        string operationName,
        ICertCorrelationContext correlationContext,
        OperationType operationType,
        TimeSpan? timeout = null,
        ICertOperationContext? parent = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));
        ArgumentNullException.ThrowIfNull(correlationContext, nameof(correlationContext));

        var operationId = Guid.NewGuid().ToString();
        var actualTimeout = timeout ?? TimeSpan.FromMinutes(5);
        var logger = _loggerFactory.CreateLogger<CertOperationContext>();

        // Create components for the operation context
        var identifier = new CertOperationIdentifier(
            operationId,
            operationName,
            operationType,
            parent?.Identifier,
            DateTimeOffset.UtcNow,
            actualTimeout);

        var state = new CertOperationState();
        var metrics = new CertOperationMetrics(identifier.Id, identifier.Name);

        // Create the validation processor with the validation result factory
        var validationProcessor = new CertValidationProcessor(
            _loggerFactory.CreateLogger<CertValidationProcessor>(),
            _validationResultFactory);

        // Create the result factory for validation
        var resultFactory = new CertResultFactory();

        // Create the validator
        var validator = new CertOperationValidator(
            validationProcessor,
            resultFactory,
            _loggerFactory.CreateLogger<CertOperationValidator>());

        // Create the resource context and resources
#pragma warning disable CA2000 // Suppresssed: ResourceManager and ResourceContext are owned by the CertOperationContext
        // and will be disposed when the context is disposed through the resources property
        var resourceManager = new CertResourceManager(_loggerFactory.CreateLogger<CertResourceManager>());
        var resourceMetrics = new CertResourceMetrics();
        var resourceContext = new CertResourceContext(
            correlationContext,
            resourceMetrics,
            resourceManager,
            _exceptionFactory,
            _loggerFactory.CreateLogger<CertResourceContext>());
#pragma warning restore CA2000

        var resources = new CertOperationResources(
            resourceContext,
            _loggerFactory.CreateLogger<CertOperationResources>());

        // Create the final operation context with all components
        var context = new CertOperationContext(
            identifier,
            state,
            metrics,
            correlationContext,
            validator,
            resources,
            logger,
            cancellationToken);

        // Set validator references
        validator.SetOperationContext(context);

        return context;
    }

    /// <summary>
    /// Creates a child context from a parent context
    /// </summary>
    /// <param name="parent">Parent context</param>
    /// <param name="childOperationName">Name of the child operation</param>
    /// <param name="operationType">Type of operation, or null to use parent's type</param>
    /// <returns>Child operation context</returns>
    public ICertOperationContext CreateChild(
        ICertOperationContext parent,
        string childOperationName,
        OperationType? operationType = null)
    {
        ArgumentNullException.ThrowIfNull(parent, nameof(parent));
        ArgumentException.ThrowIfNullOrEmpty(childOperationName, nameof(childOperationName));

        var context = CreateConfigured(
            childOperationName,
            parent.CorrelationContext,
            operationType ?? parent.Identifier.OperationType,
            parent.Identifier.Timeout,
            parent,
            parent.CancellationToken);

        var logger = _loggerFactory.CreateLogger<CertOperationContextFactory>();
        LogCreatedChildContext(
            logger,
            childOperationName,
            parent.Identifier.Id,
            parent.CorrelationContext.CorrelationId,
            null);

        return context;
    }

    /// <summary>
    /// Creates a standalone context with its own correlation context
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="operationType">Type of operation</param>
    /// <param name="timeout">Operation timeout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation context</returns>
    public ICertOperationContext CreateStandalone(
        string operationName,
        OperationType operationType = OperationType.Generic,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

        // Create a new correlation context
        var correlationId = _correlationIdFactory.Create();
        var logger = _loggerFactory.CreateLogger<CertCorrelationContext>();
        var correlationContext = new CertCorrelationContext(correlationId, _correlationIdFactory, logger);

        return CreateConfigured(
            operationName,
            correlationContext,
            operationType,
            timeout,
            null,
            cancellationToken);
    }

    /// <summary>
    /// Creates an initialized context
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation context</returns>
    public async Task<ICertOperationContext> CreateInitializedContextAsync(CancellationToken cancellationToken = default)
    {
        // Create a standalone context with default settings
        var context = CreateStandalone(
            "InitializedContext",
            OperationType.Generic,
            null,
            cancellationToken);

        // Initialize the context
        await context.InitializeAsync(cancellationToken).ConfigureAwait(false);

        return context;
    }
}
