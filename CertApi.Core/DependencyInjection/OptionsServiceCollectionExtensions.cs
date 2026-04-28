// CertApi.Core/DependencyInjection/OptionsServiceCollectionExtensions.cs
using CertApi.Core.Authentication;
using CertApi.Core.Configuration;
using CertApi.Core.Resilience;
using CertApi.Core.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CertApi.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering options with the service collection
/// </summary>
public static class OptionsServiceCollectionExtensions
{
    /// <summary>
    /// Adds all core options services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Optional configuration to bind from</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreOptions(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Add simple options with data annotations validation
        services.AddCertOptions<CertTimeoutOptions>(configuration);
        services.AddCertOptions<CertRetryOptions>(configuration);
        services.AddCertOptions<CertBulkheadOptions>(configuration);
        services.AddCertOptions<CertCircuitBreakerOptions>(configuration);
        services.AddCertOptions<CertCacheOptions>(configuration);
        services.AddCertOptions<CertBatchOptions>(configuration);
        services.AddCertOptions<CertClientOptions>(configuration);
        services.AddCertOptions<CertClientCertificateOptions>(configuration);
        services.AddCertOptions<CertValidationOptions>(configuration);
        services.AddCertOptions<CertCorrelationContextOptions>(configuration);

        // Add options with custom validators
        services.AddCertOptionsWithValidator<CertAuthenticationOptions, CertAuthenticationOptionsValidator>(configuration);
        services.AddCertOptionsWithValidator<CertConnectionOptions, CertConnectionOptionsValidator>(configuration);
        services.AddCertOptionsWithValidator<CertPolicyOptions, CertPolicyOptionsValidator>(configuration);

        return services;
    }

    /// <summary>
    /// Adds options with configuration binding and data annotations validation
    /// </summary>
    /// <typeparam name="TOptions">Options type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Optional configuration to bind from</param>
    /// <param name="sectionName">Optional section name</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCertOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        string? sectionName = null)
        where TOptions : class, new()
    {
        // Use type name as section name if not provided
        sectionName ??= typeof(TOptions).Name
            .Replace("Cert", "", StringComparison.Ordinal)
            .Replace("Options", "", StringComparison.Ordinal);

        // Configure the options
        var optionsBuilder = services.AddOptions<TOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Bind to configuration if provided
        if (configuration != null)
        {
            optionsBuilder.Bind(configuration.GetSection(sectionName));
        }

        return services;
    }

    /// <summary>
    /// Adds options with configuration binding, data annotations validation, and custom validator
    /// </summary>
    /// <typeparam name="TOptions">Options type</typeparam>
    /// <typeparam name="TValidator">Validator type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Optional configuration to bind from</param>
    /// <param name="sectionName">Optional section name</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCertOptionsWithValidator<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        string? sectionName = null)
        where TOptions : class, new()
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddCertOptions<TOptions>(configuration, sectionName);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, TValidator>());

        return services;
    }
}