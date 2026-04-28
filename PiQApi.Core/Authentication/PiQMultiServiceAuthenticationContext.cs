// PiQApi.Core/Authentication/PiQMultiServiceAuthenticationContext.cs
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace PiQApi.Core.Authentication;

/// <summary>
/// Implementation of the multi-service authentication context
/// </summary>
public class PiQMultiServiceAuthenticationContext : IPiQMultiServiceAuthenticationContext
{
    private readonly IPiQAuthenticationProviderFactory _authProviderFactory;
    private readonly IPiQExceptionFactory _exceptionFactory;
    private readonly ILogger<PiQMultiServiceAuthenticationContext> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<PiQCacheOptions> _cacheOptions;
    private readonly IOptions<PiQClientOptions> _clientOptions;
    private readonly ConcurrentDictionary<ServiceEndpointType, IPiQAuthenticationContext> _authContexts = new();

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, ServiceEndpointType, string, Exception?> LogGettingAuthContext =
        LoggerMessage.Define<ServiceEndpointType, string>(
            LogLevel.Debug,
            new EventId(1, nameof(GetAuthenticationContext)),
            "Getting authentication context for service {ServiceType}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, ServiceEndpointType, string, Exception?> LogNoAuthContext =
        LoggerMessage.Define<ServiceEndpointType, string>(
            LogLevel.Warning,
            new EventId(2, nameof(GetAuthenticationContext)),
            "No authentication context registered for service {ServiceType}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, ServiceEndpointType, string, Exception?> LogAcquiringToken =
        LoggerMessage.Define<ServiceEndpointType, string>(
            LogLevel.Debug,
            new EventId(3, nameof(AcquireTokenAsync)),
            "Acquiring token for service {ServiceType}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, ServiceEndpointType, string, Exception?> LogTokenAcquired =
        LoggerMessage.Define<ServiceEndpointType, string>(
            LogLevel.Information,
            new EventId(4, nameof(AcquireTokenAsync)),
            "Successfully acquired token for service {ServiceType}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, ServiceEndpointType, string, Exception?> LogRegisteringService =
        LoggerMessage.Define<ServiceEndpointType, string>(
            LogLevel.Debug,
            new EventId(5, nameof(RegisterServiceAsync)),
            "Registering service {ServiceType}, CorrelationId: {CorrelationId}");

    private static readonly Action<ILogger, ServiceEndpointType, string, Exception?> LogServiceRegistered =
        LoggerMessage.Define<ServiceEndpointType, string>(
            LogLevel.Information,
            new EventId(6, nameof(RegisterServiceAsync)),
            "Service {ServiceType} registered successfully, CorrelationId: {CorrelationId}");

    /// <summary>
    /// Gets the correlation context
    /// </summary>
    public IPiQCorrelationContext CorrelationContext { get; }

    /// <summary>
    /// Gets all registered service endpoints
    /// </summary>
    public IReadOnlyDictionary<ServiceEndpointType, IPiQAuthenticationContext> RegisteredServices => _authContexts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQMultiServiceAuthenticationContext"/> class
    /// </summary>
    /// <param name="authProviderFactory">Authentication provider factory</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <param name="logger">Logger</param>
    /// <param name="loggerFactory">Logger factory</param>
    /// <param name="exceptionFactory">Exception factory</param>
    /// <param name="cacheOptions">Cache options</param>
    /// <param name="clientOptions">Client options</param>
    public PiQMultiServiceAuthenticationContext(
        IPiQAuthenticationProviderFactory authProviderFactory,
        IPiQCorrelationContext correlationContext,
        ILogger<PiQMultiServiceAuthenticationContext> logger,
        ILoggerFactory loggerFactory,
        IPiQExceptionFactory exceptionFactory,
        IOptions<PiQCacheOptions> cacheOptions,
        IOptions<PiQClientOptions> clientOptions)
    {
        _authProviderFactory = authProviderFactory ?? throw new ArgumentNullException(nameof(authProviderFactory));
        CorrelationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
        _clientOptions = clientOptions ?? throw new ArgumentNullException(nameof(clientOptions));
    }

    /// <summary>
    /// Gets the authentication context for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <returns>Authentication context for the service</returns>
    /// <exception cref="InvalidOperationException">Thrown when no authentication context is registered for the service</exception>
    public IPiQAuthenticationContext GetAuthenticationContext(ServiceEndpointType serviceType)
    {
        LogGettingAuthContext(_logger, serviceType, CorrelationContext.CorrelationId, null);

        if (_authContexts.TryGetValue(serviceType, out var authContext))
        {
            return authContext;
        }

        LogNoAuthContext(_logger, serviceType, CorrelationContext.CorrelationId, null);
        throw _exceptionFactory.CreateConfigurationException(
            $"No authentication context registered for service {serviceType}",
            "ServiceNotRegistered");
    }

    /// <summary>
    /// Acquires a token for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token for the service</returns>
    public async Task<string> AcquireTokenAsync(ServiceEndpointType serviceType, CancellationToken cancellationToken = default)
    {
        LogAcquiringToken(_logger, serviceType, CorrelationContext.CorrelationId, null);

        // Get the authentication context for the service
        var authContext = GetAuthenticationContext(serviceType);

        // Acquire a token from the context
        var token = await authContext.AcquireTokenAsync(cancellationToken).ConfigureAwait(false);

        LogTokenAcquired(_logger, serviceType, CorrelationContext.CorrelationId, null);
        return token;
    }

    /// <summary>
    /// Registers a new service with authentication options
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="options">Authentication options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RegisterServiceAsync(ServiceEndpointType serviceType, IPiQAuthenticationOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        LogRegisteringService(_logger, serviceType, CorrelationContext.CorrelationId, null);

        // Create an authentication provider for the options
        var authProvider = _authProviderFactory.CreateProvider(options);

        // Create an authentication context for the provider
        var contextLogger = _loggerFactory.CreateLogger<PiQAuthenticationContext>();
        var authContext = new PiQAuthenticationContext(
            contextLogger,
            _exceptionFactory,
            authProvider,
            _cacheOptions,
            _clientOptions);

        // Register the context
        _authContexts[serviceType] = authContext;

        LogServiceRegistered(_logger, serviceType, CorrelationContext.CorrelationId, null);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Validates the token for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the token is valid; otherwise, false</returns>
    public async Task<bool> ValidateTokenAsync(ServiceEndpointType serviceType, CancellationToken cancellationToken = default)
    {
        // Get the authentication context for the service
        var authContext = GetAuthenticationContext(serviceType);

        // Validate the token
        return await authContext.ValidateTokenAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Invalidates the token for a specific service
    /// </summary>
    /// <param name="serviceType">Service endpoint type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task InvalidateTokenAsync(ServiceEndpointType serviceType, CancellationToken cancellationToken = default)
    {
        // Get the authentication context for the service
        var authContext = GetAuthenticationContext(serviceType);

        // Invalidate the token
        await authContext.InvalidateTokenAsync(cancellationToken).ConfigureAwait(false);
    }
}