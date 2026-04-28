// PiQApi.Core/DependencyInjection/OptionsServiceCollectionExtensions.cs
using PiQApi.Core.Authentication;
using PiQApi.Core.Configuration;
using PiQApi.Core.Resilience;
using PiQApi.Core.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace PiQApi.Core.DependencyInjection;

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
        services.AddPiQOptions<PiQTimeoutOptions>(configuration);
        services.AddPiQOptions<PiQRetryOptions>(configuration);
        services.AddPiQOptions<PiQBulkheadOptions>(configuration);
        services.AddPiQOptions<PiQCircuitBreakerOptions>(configuration);
        services.AddPiQOptions<PiQCacheOptions>(configuration);
        services.AddPiQOptions<PiQBatchOptions>(configuration);
        services.AddPiQOptions<PiQClientOptions>(configuration);
        services.AddPiQOptions<PiQClientCertificateOptions>(configuration);
        services.AddPiQOptions<PiQValidationOptions>(configuration);
        services.AddPiQOptions<PiQCorrelationContextOptions>(configuration);

        // Add options with custom validators
        services.AddPiQOptionsWithValidator<PiQAuthenticationOptions, PiQAuthenticationOptionsValidator>(configuration);
        services.AddPiQOptionsWithValidator<PiQConnectionOptions, PiQConnectionOptionsValidator>(configuration);
        services.AddPiQOptionsWithValidator<PiQPolicyOptions, PiQPolicyOptionsValidator>(configuration);

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
    public static IServiceCollection AddPiQOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        string? sectionName = null)
        where TOptions : class, new()
    {
        // Use type name as section name if not provided
        sectionName ??= typeof(TOptions).Name
            .Replace("PiQ", "", StringComparison.Ordinal)
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
    public static IServiceCollection AddPiQOptionsWithValidator<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        string? sectionName = null)
        where TOptions : class, new()
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddPiQOptions<TOptions>(configuration, sectionName);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, TValidator>());

        return services;
    }
}
