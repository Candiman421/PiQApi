// CertApi.Core/Authentication/CertAuthenticationContext.cs
using CertApi.Abstractions.Authentication;
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Utilities.Time;
using CertApi.Core.Configuration;
using CertApi.Core.Utilities.Time;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CertApi.Core.Authentication;

/// <summary>
/// Implementation of the authentication context for maintaining token state
/// </summary>
public sealed class CertAuthenticationContext : ICertAuthenticationContext
{
    private readonly ILogger<CertAuthenticationContext> _logger;
    private readonly ICertExceptionFactory _exceptionFactory;
    private readonly ICertTokenProvider _tokenProvider;
    private readonly CertCacheOptions _certCacheOptions;
    private readonly CertClientOptions _certClientOptions;
    private readonly ICertTimeProvider _timeProvider;

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
    /// Initializes a new instance of the <see cref="CertAuthenticationContext"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="exceptionFactory">The exception factory</param>
    /// <param name="tokenProvider">The token provider</param>
    /// <param name="certCacheOptions">The cache options</param>
    /// <param name="certClientOptions">The client options</param>
    /// <param name="timeProvider">Time provider for improved testability</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public CertAuthenticationContext(
        ILogger<CertAuthenticationContext> logger,
        ICertExceptionFactory exceptionFactory,
        ICertTokenProvider tokenProvider,
        IOptions<CertCacheOptions> certCacheOptions,
        IOptions<CertClientOptions> certClientOptions,
        ICertTimeProvider? timeProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _certCacheOptions = certCacheOptions?.Value ?? throw new ArgumentNullException(nameof(certCacheOptions));
        _certClientOptions = certClientOptions?.Value ?? throw new ArgumentNullException(nameof(certClientOptions));
        _timeProvider = timeProvider ?? new CertSystemTimeProvider();
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
            TokenExpiration = _timeProvider.UtcNow.AddMinutes(_certCacheOptions.TokenExpirationMinutes);

            LogTokenAcquired(_logger, TokenExpiration, null);

            return AccessToken;
        }
        catch (Exception ex)
        {
            if (_certClientOptions.ServiceUri != null)
            {
                throw _exceptionFactory.CreateAuthenticationException(
                    "Failed to acquire access token",
                    "TokenAcquisitionFailed",
                    _certClientOptions.ServiceUri,
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
            TokenExpiration = _timeProvider.UtcNow.AddMinutes(_certCacheOptions.TokenExpirationMinutes);
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
