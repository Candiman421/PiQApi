// CertApi.Core/DependencyInjection/ContextServiceCollectionExtensions.cs
using CertApi.Abstractions.Context;
using CertApi.Abstractions.Core;
using CertApi.Abstractions.Factories;
using CertApi.Core.Context;
using CertApi.Core.Core;
using CertApi.Core.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CertApi.Core.DependencyInjection;

/// <summary>
/// Extension methods for setting up context-related services in an IServiceCollection
/// </summary>
public static class ContextServiceCollectionExtensions
{
    /// <summary>
    /// Adds correlation and operation context services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddContextServices(this IServiceCollection services)
    {
        // Register common context services
        AddCorrelationServices(services);
        AddOperationServices(services);

        return services;
    }

    /// <summary>
    /// Adds operation context services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOperationContextServices(this IServiceCollection services)
    {
        // Ensure correlation services are registered
        AddCorrelationServices(services);
        AddOperationServices(services);

        return services;
    }

    /// <summary>
    /// Configures correlation context services in the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure correlation options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ConfigureCorrelationContext(
        this IServiceCollection services,
        Action<Configuration.CertCorrelationContextOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Add options
        services.Configure(configureOptions);

        return services;
    }

    /// <summary>
    /// Adds correlation-related services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void AddCorrelationServices(IServiceCollection services)
    {
        services.TryAddSingleton<ICertCorrelationIdFactory, CertCorrelationIdFactory>();
        services.TryAddScoped<ICertCorrelationContext, CertCorrelationContext>();
        services.TryAddTransient<ICertCorrelationScope, CertCorrelationScope>();
    }

    /// <summary>
    /// Adds operation-related services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void AddOperationServices(IServiceCollection services)
    {
        services.TryAddSingleton<ICertOperationContextFactory, CertOperationContextFactory>();
        services.TryAddScoped<ICertOperationMetrics, CertOperationMetrics>();
        services.TryAddScoped<ICertOperationState, CertOperationState>();
        services.TryAddTransient<ICertOperationScope, CertOperationScope>();
    }
}