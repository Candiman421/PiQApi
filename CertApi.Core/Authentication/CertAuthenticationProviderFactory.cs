// CertApi.Core/Authentication/CertAuthenticationProviderFactory.cs
using CertApi.Abstractions.Authentication;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Enums;
using CertApi.Abstractions.Factories;
using Microsoft.Extensions.Logging;

namespace CertApi.Core.Authentication;

/// <summary>
/// Factory for creating authentication providers
/// </summary>
public class CertAuthenticationProviderFactory : ICertAuthenticationProviderFactory
{
    // Logger message delegates for better performance
    private static readonly Action<ILogger, string, string, Exception?> LogCreatingProvider =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(CreateProvider)),
            "[{CorrelationId}] Creating authentication provider for type: {AuthType}");

    private static readonly Action<ILogger, string, string, Exception?> LogProviderCreationError =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(2, nameof(CreateProvider)),
            "[{CorrelationId}] Error creating authentication provider for type: {AuthType}");

    private readonly ILogger<CertAuthenticationProviderFactory> _logger;
    private readonly ICertExceptionFactory _exceptionFactory;
    private readonly IDictionary<AuthenticationMethodType, Func<ICertAuthenticationProvider>> _providerFactories;

    /// <summary>
    /// Gets the correlation context
    /// </summary>
    public ICertCorrelationContext CorrelationContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertAuthenticationProviderFactory"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="exceptionFactory">The exception factory</param>
    /// <param name="providerFactories">Dictionary of provider factory functions by authentication type</param>
    /// <param name="correlationContext">Correlation context</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null</exception>
    public CertAuthenticationProviderFactory(
        ILogger<CertAuthenticationProviderFactory> logger,
        ICertExceptionFactory exceptionFactory,
        IDictionary<AuthenticationMethodType, Func<ICertAuthenticationProvider>> providerFactories,
        ICertCorrelationContext correlationContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        _providerFactories = providerFactories ?? throw new ArgumentNullException(nameof(providerFactories));
        CorrelationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
    }

    /// <summary>
    /// Creates an authentication provider for the specified authentication type
    /// </summary>
    /// <param name="authType">Authentication type</param>
    /// <returns>Authentication provider</returns>
    /// <exception cref="NotSupportedException">Thrown when the authentication type is not supported</exception>
    /// <exception cref="Exception">Thrown when provider creation fails</exception>
    public virtual ICertAuthenticationProvider CreateProvider(AuthenticationMethodType authType)
    {
        LogCreatingProvider(_logger, CorrelationContext.CorrelationId, authType.ToString(), null);

        // Track provider creation in correlation context
        CorrelationContext.AddProperty("AuthProviderType", authType.ToString());
        CorrelationContext.AddProperty("OperationType", "ProviderCreation");

        try
        {
            if (_providerFactories.TryGetValue(authType, out var factory))
            {
                var provider = factory();
                CorrelationContext.AddProperty("AuthProviderCreated", provider.GetType().Name);
                return provider;
            }

            var errorMessage = $"Authentication type '{authType}' is not supported";
            CorrelationContext.AddProperty("AuthProviderError", errorMessage);
            throw new NotSupportedException(errorMessage);
        }
        catch (Exception ex) when (ex is not NotSupportedException)
        {
            LogProviderCreationError(_logger, CorrelationContext.CorrelationId, authType.ToString(), ex);

            // Add exception info to correlation context
            CorrelationContext.AddProperty("AuthProviderError", ex.Message);
            CorrelationContext.AddProperty("AuthProviderErrorType", ex.GetType().Name);

            // Add correlation ID to exception data
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationContext.CorrelationId))
            {
                ex.Data["CorrelationId"] = CorrelationContext.CorrelationId;
                ex.Data["AuthType"] = authType.ToString();
            }

            throw _exceptionFactory.CreateAuthenticationException(
                $"Failed to create authentication provider for type '{authType}'",
                "ProviderCreationFailed",
                null, // serviceUri
                authType.ToString(), // tokenId - using authType as an identifier
                ex);
        }
    }

    /// <summary>
    /// Creates an authentication provider using the authentication type from options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <returns>Authentication provider</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public ICertAuthenticationProvider CreateProvider(ICertAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        CorrelationContext.AddProperty("AuthOptionsType", options.GetType().Name);
        CorrelationContext.AddProperty("RequestedAuthType", options.AuthType.ToString());

        if (options.ClientId != null)
        {
            CorrelationContext.AddProperty("ClientId", options.ClientId);
        }

        if (options.TenantId != null)
        {
            CorrelationContext.AddProperty("TenantId", options.TenantId);
        }

        return CreateProvider(options.AuthType);
    }

    /// <summary>
    /// Creates an authentication provider using the authentication options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <returns>Authentication provider</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public ICertAuthenticationProvider CreateProvider(CertAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        CorrelationContext.AddProperty("AuthOptionsType", nameof(CertAuthenticationOptions));
        CorrelationContext.AddProperty("RequestedAuthType", options.AuthType.ToString());

        if (options.ClientId != null)
        {
            CorrelationContext.AddProperty("ClientId", options.ClientId);
        }

        if (options.TenantId != null)
        {
            CorrelationContext.AddProperty("TenantId", options.TenantId);
        }

        return CreateProvider(options.AuthType);
    }
}