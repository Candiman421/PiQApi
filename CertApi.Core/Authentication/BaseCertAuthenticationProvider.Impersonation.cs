// CertApi.Core/Authentication/BaseCertAuthenticationProvider.Impersonation.cs
namespace CertApi.Core.Authentication;

public abstract partial class BaseCertAuthenticationProvider
{
    /// <summary>
    /// Determines whether impersonation is supported
    /// </summary>
    public virtual Task<bool> SupportsImpersonationAsync(CancellationToken ct = default)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Configures impersonation for the specified user
    /// </summary>
    public virtual Task ConfigureImpersonationAsync(string userId, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        // Track impersonation attempt in correlation context
        CorrelationContext.AddProperty("ImpersonationAttempt", userId);
        CorrelationContext.AddProperty("AuthType", AuthType.ToString());

        // Get correlation ID to ensure consistent correlation tracking
        string correlationId = CorrelationContext.CorrelationId;

        // Log the impersonation attempt using the LoggerMessage delegate
        LogImpersonationNotSupported(Logger, correlationId, AuthType.ToString(), userId, null);

        // Since we can't return a result object directly due to interface constraints,
        // we'll throw an exception for the unsupported operation
        var exception = new NotSupportedException($"Impersonation is not supported by this authentication provider. CorrelationId: {correlationId}");
        exception.Data["CorrelationId"] = correlationId;
        exception.Data["AuthType"] = AuthType.ToString();
        exception.Data["UserId"] = userId;
        throw exception;
    }
}