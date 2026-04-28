// PiQApi.Core/Authentication/PiQAuthenticationContext.cs
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Core.Configuration;
using PiQApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PiQApi.Core.Authentication;

/// <summary>
/// Implementation of the authentication context for maintaining token state
/// </summary>
public sealed class PiQAuthenticationContext : IPiQAuthenticationContext
{
    private readonly ILogger<PiQAuthenticationContext> _logger;
    private readonly IPiQExceptionFactory _exceptionFactory;
    private readonly IPiQTokenProvider _tokenProvider;
    private readonly PiQCacheOptions _piqCacheOptions;
    private readonly PiQClientOptions _piqClientOptions;
    private readonly IPiQTimeProvider _timeProvider;

    // LoggerMessage delegates for better performance
    private static readonly Action<ILogger, DateTime, Exception?> LogTokenAcquired =
        LoggerMessage.Define<DateTime>(
            LogLevel.Information,
            new EventId(1, "AcquireTokenAsync"),
            "Successfully acquired new access token. Expires: {ExpirationTime}");

    private static readonly Action<ILogger, Exception?> LogTokenValidationFailed =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2, "ValidateTokenAsync"),
            "Token validation failed - unable to refresh token");

    private static readonly Action<ILogger, Exception?> LogTokenValidationError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(3, "ValidateTokenAsync"),
            "Token validation failed");

    private static readonly Action<ILogger, Exception?> LogTokenInvalidated =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(4, "InvalidateTokenAsync"),
            "Token invalidated");

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQAuthenticationContext"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="exceptionFactory">The exception factory</param>
    /// <param name="tokenProvider">The token provider</param>
    /// <param name="piqCacheOptions">The cache options</param>
    /// <param name="piqClientOptions">The client options</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public PiQAuthenticationContext(
        ILogger<PiQAuthenticationContext> logger,
        IPiQExceptionFactory exceptionFactory,
        IPiQTokenProvider tokenProvider,
        IOptions<PiQCacheOptions> piqCacheOptions,
        IOptions<PiQClientOptions> piqClientOptions,
        IPiQTimeProvider? timeProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _piqCacheOptions = piqCacheOptions?.Value ?? throw new ArgumentNullException(nameof(piqCacheOptions));
        _piqClientOptions = piqClientOptions?.Value ?? throw new ArgumentNullException(nameof(piqClientOptions));
        _timeProvider = timeProvider ?? new PiQSystemTimeProvider();
    }

    /// <summary>
    /// Gets the current access token
    /// </summary>
    public string AccessToken { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the context is authenticated
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken) && TokenExpiration > _timeProvider.UtcNow;

    /// <summary>
    /// Gets the token expiration time
    /// </summary>
    public DateTime TokenExpiration { get; private set; }

    /// <summary>
    /// Acquires an access token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token</returns>
    /// <exception cref="Exception">Thrown when token acquisition fails</exception>
    public async Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsAuthenticated && TokenExpiration > _timeProvider.UtcNow.AddMinutes(5))
            {
                return AccessToken;
            }

            AccessToken = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            TokenExpiration = _timeProvider.UtcNow.AddMinutes(_piqCacheOptions.TokenExpirationMinutes);

            LogTokenAcquired(_logger, TokenExpiration, null);

            return AccessToken;
        }
        catch (Exception ex)
        {
            if (_piqClientOptions.ServiceUri != null)
            {
                throw _exceptionFactory.CreateAuthenticationException(
                    "Failed to acquire access token",
                    "TokenAcquisitionFailed",
                    _piqClientOptions.ServiceUri,
                    AccessToken,
                    ex);
            }
            else
            {
                // Default URI for error handling when ServiceUri is null
                var defaultUri = new Uri("https://unknown.service");
                throw _exceptionFactory.CreateAuthenticationException(
                    "Failed to acquire access token (service URI not configured)",
                    "TokenAcquisitionFailed",
                    defaultUri,
                    AccessToken,
                    ex);
            }
        }
    }

    /// <summary>
    /// Validates the current token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is valid; otherwise, false</returns>
    public async Task<bool> ValidateTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
            return false;

        try
        {
            var refreshedToken = await _tokenProvider.RefreshTokenAsync(AccessToken, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(refreshedToken))
            {
                LogTokenValidationFailed(_logger, null);
                return false;
            }

            AccessToken = refreshedToken;
            TokenExpiration = _timeProvider.UtcNow.AddMinutes(_piqCacheOptions.TokenExpirationMinutes);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            // Catch specific exception when token is invalid
            LogTokenValidationError(_logger, ex);
            return false;
        }
        catch (TimeoutException ex)
        {
            // Catch specific exception for timeouts
            LogTokenValidationError(_logger, ex);
            return false;
        }
        catch (ArgumentException ex)
        {
            // Catch specific exception for invalid arguments
            LogTokenValidationError(_logger, ex);
            return false;
        }
        catch (HttpRequestException ex)
        {
            // Catch specific exception for network issues
            LogTokenValidationError(_logger, ex);
            return false;
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException and
            not TaskCanceledException)
        {
            // More specific filtering on the general exception
            LogTokenValidationError(_logger, ex);
            return false;
        }
        // Let cancellation exceptions propagate
    }

    /// <summary>
    /// Invalidates the current token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task InvalidateTokenAsync(CancellationToken cancellationToken = default)
    {
        AccessToken = string.Empty;
        TokenExpiration = DateTime.MinValue;
        LogTokenInvalidated(_logger, null);
        return Task.CompletedTask;
    }
}
