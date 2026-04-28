// PiQApi.Core/DependencyInjection/CoreServiceCollectionExtensions.cs
using PiQApi.Abstractions.Authentication;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Monitoring;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Authentication.TokenCache;
using PiQApi.Core.Authentication.TokenCache.Interfaces;
using PiQApi.Core.Authentication.Validation;
using PiQApi.Core.Configuration;
using PiQApi.Core.Context;
using PiQApi.Core.Factories;
using PiQApi.Core.Monitoring;
using PiQApi.Core.Results;
using PiQApi.Core.Threading;
using PiQApi.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using PiQApi.Abstractions.Results;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PiQApi.Core.Authentication;
using PiQApi.Core.Utilities.Time;
using PiQApi.Core.Utilities.RandomProviders;
using PiQApi.Abstractions.Utilities.Time;
using PiQApi.Abstractions.Utilities.Randomization;

namespace PiQApi.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering core services
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds all core services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAllCoreServices(this IServiceCollection services)
    {
        return services
            .AddCoreServices()
            .AddMetricsServices()
            .AddValidationServices();
    }

    /// <summary>
    /// Adds core infrastructure services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Register core factories
        services.TryAddSingleton<ICertCorrelationIdFactory, CertCorrelationIdFactory>();
        services.TryAddSingleton<ICertExceptionFactory, CertExceptionFactory>();
        services.TryAddSingleton<ICertResultFactory, CertResultFactory>();
        services.TryAddSingleton<ICertResultTransformer, CertResultTransformer>();
        services.TryAddSingleton<ICertAsyncLockFactory, CertAsyncLockFactory>();

        // Register time and random providers
        services.TryAddSingleton<ICertTimeProvider, CertSystemTimeProvider>();
        services.TryAddSingleton<ICertRandomProvider>(sp =>
            CertRandomProviderFactory.SecureProvider); // Default to secure

        // Also register the factory for custom provider creation
        services.TryAddSingleton<ICertTimeProviderFactory, CertTimeProviderFactory>();

        // Configure CertTimeProviderFactory with the real implementation
        // This is done during service registration to ensure all static contexts
        // use the same time provider as DI contexts
        using (var tempProvider = services.BuildServiceProvider())
        {
            var timeProvider = tempProvider.GetRequiredService<ICertTimeProvider>();
            // Use the static ConfigureStatic method, not the instance method
            CertTimeProviderFactory.ConfigureStatic(timeProvider);
        }

        // Register authentication services
        services.TryAddScoped<ICertAuthenticationContext, CertAuthenticationContext>();

        // Register token cache and validator interfaces
        services.TryAddSingleton<ICertTokenCache, CertMemoryTokenCache>();
        services.TryAddScoped<ICertTokenValidator, CertTokenValidator>();

        // Register token cache options - using proper record initialization
        services.AddOptions<CertCacheOptions>()
            .Configure(options =>
            {
                // For records, use the 'with' expression to create a new instance
                var defaultOptions = new CertCacheOptions
                {
                    DefaultExpirationMinutes = 60,
                    MaxItems = 1000
                };

                // Copy the values to the options instance - this is a workaround
                // since we can't directly modify the options instance with init properties
                typeof(CertCacheOptions).GetProperty(nameof(CertCacheOptions.DefaultExpirationMinutes))
                    ?.SetValue(options, defaultOptions.DefaultExpirationMinutes);
                typeof(CertCacheOptions).GetProperty(nameof(CertCacheOptions.MaxItems))
                    ?.SetValue(options, defaultOptions.MaxItems);
            })
            .ValidateDataAnnotations();

        return services;
    }

    /// <summary>
    /// Adds metrics services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMetricsServices(this IServiceCollection services)
    {
        // Register individual trackers
        services.TryAddSingleton<ICertConnectionMetrics, CertConnectionMetricsTracker>();
        services.TryAddSingleton<ICertOperationMetrics, CertOperationMetrics>();
        services.TryAddSingleton<ICertCacheMetrics, CertCacheMetricsTracker>();

        // Register composite tracker
        services.TryAddSingleton<ICertCompositeMetricsTracker, CertCompositeMetricsTracker>();

        // Register interface resolvers
        services.TryAddSingleton<ICertMetricsProvider>(sp =>
            sp.GetRequiredService<ICertCompositeMetricsTracker>());

        return services;
    }

    /// <summary>
    /// Adds validation services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ICertValidationProcessor, CertValidationProcessor>();
        services.TryAddSingleton<ICertValidationRuleFactory, CertValidationRuleFactory>();

        // Add validation diagnostics service
        services.TryAddSingleton<ICertValidationDiagnosticsService, Validation.Diagnostics.CertValidationDiagnosticsService>();

        // Ensure validation context factory is registered
        services.TryAddSingleton<ICertValidationContextFactory, CertValidationContextFactory>();

        return services;
    }
}