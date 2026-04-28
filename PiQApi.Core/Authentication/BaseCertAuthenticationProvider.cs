// PiQApi.Core/Authentication/BasePiQAuthenticationProvider.cs
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Enums;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Authentication;

/// <summary>
/// Base implementation for authentication providers
/// </summary>
public abstract partial class BasePiQAuthenticationProvider : IPiQAuthenticationProvider
{
    // Logger message delegates for better performance
    private static readonly Action<ILogger, string, string, Exception?> LogValidatingToken =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(ValidateTokenAsync)),
            "[{CorrelationId}] Validating token for client {ClientId}");

    private static readonly Action<ILogger, string, bool, string, Exception?> LogValidationResult =
        LoggerMessage.Define<string, bool, string>(
            LogLevel.Debug,
            new EventId(2, nameof(ValidateTokenAsync)),
            "[{CorrelationId}] Token validation result: {IsValid} for client {ClientId}");

    private static readonly Action<ILogger, string, string, Exception?> LogInvalidatingToken =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(3, nameof(InvalidateTokenAsync)),
            "[{CorrelationId}] Invalidating token for client {ClientId}");

    private static readonly Action<ILogger, string, string, Exception?> LogRefreshingToken =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(4, nameof(RefreshTokenAsync)),
            "[{CorrelationId}] Refreshing token for client {ClientId}");

    private static readonly Action<ILogger, string, string, Exception?> LogCannotRefresh =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(5, nameof(RefreshTokenAsync)),
            "[{CorrelationId}] Cannot refresh token without source options for client {ClientId}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogImpersonationNotSupported =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(6, nameof(ConfigureImpersonationAsync)),
            "[{CorrelationId}] Impersonation not supported by this authentication provider (AuthType: {AuthType}), User: {UserId}");

    private static readonly Action<ILogger, string, string, string, Exception?> LogTokenAcquired =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            new EventId(7, nameof(GetTokenAsync)),
            "[{CorrelationId}] Successfully acquired token for client {ClientId} with auth type {AuthType}");

    /// <summary>
    /// Gets the logger
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the correlation context
    /// </summary>
    public IPiQCorrelationContext CorrelationContext { get; }

    /// <summary>
    /// Gets the result factory
    /// </summary>
    protected IPiQResultFactory ResultFactory { get; }

    /// <summary>
    /// Gets the time provider
    /// </summary>
    protected IPiQTimeProvider TimeProvider { get; }

    /// <summary>
    /// Gets the authentication type
    /// </summary>
    public abstract AuthenticationMethodType AuthType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasePiQAuthenticationProvider"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="correlationContext">The correlation context</param>
    /// <param name="resultFactory">The result factory</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    protected BasePiQAuthenticationProvider(
        ILogger logger,
        IPiQCorrelationContext correlationContext,
        IPiQResultFactory resultFactory,
        IPiQTimeProvider? timeProvider = null)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CorrelationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        ResultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        TimeProvider = timeProvider ?? PiQTimeProviderFactory.Current;
    }

    /// <summary>
    /// Gets a token using the specified options
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication token info</returns>
    public virtual async Task<IPiQTokenInfo> GetTokenAsync(IPiQAuthenticationOptions options, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Track operation details in correlation context
        CorrelationContext.AddProperty("AuthOperation", "GetToken");
        CorrelationContext.AddProperty("AuthType", AuthType.ToString());

        if (options.ClientId != null)
        {
            CorrelationContext.AddProperty("ClientId", options.ClientId);
        }

        if (options.TenantId != null)
        {
            CorrelationContext.AddProperty("TenantId", options.TenantId);
        }

        try
        {
            var token = await GetTokenInternalAsync(options, ct).ConfigureAwait(false);

            // Log successful token acquisition
            LogTokenAcquired(Logger, CorrelationContext.CorrelationId,
                options.ClientId ?? "unknown", AuthType.ToString(), null);

            // Track additional token details
            CorrelationContext.AddProperty("TokenAcquired", true);
            CorrelationContext.AddProperty("TokenExpiration", token.ExpiresOn.ToString("o"));

            return token;
        }
        catch (Exception ex)
        {
            // Track failure in correlation context
            CorrelationContext.AddProperty("TokenAcquired", false);
            CorrelationContext.AddProperty("TokenError", ex.Message);
            CorrelationContext.AddProperty("TokenErrorType", ex.GetType().Name);

            // Add correlation ID to exception data if not already present
            if (!ex.Data.Contains("CorrelationId") && !string.IsNullOrEmpty(CorrelationContext.CorrelationId))
            {
                ex.Data["CorrelationId"] = CorrelationContext.CorrelationId;
                ex.Data["AuthType"] = AuthType.ToString();
                ex.Data["ClientId"] = options.ClientId;
            }

            throw;
        }
    }

    /// <summary>
    /// Gets a token using the specified options (internal implementation)
    /// </summary>
    /// <param name="options">Authentication options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication token info</returns>
    protected abstract Task<IPiQTokenInfo> GetTokenInternalAsync(IPiQAuthenticationOptions options, CancellationToken ct);
}
