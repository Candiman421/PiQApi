// PiQApi.Core/DependencyInjection/ContextServiceCollectionExtensions.cs
using PiQApi.Abstractions.Context;
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Context;
using PiQApi.Core.Core;
using PiQApi.Core.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PiQApi.Core.DependencyInjection;

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
        Action<Configuration.PiQCorrelationContextOptions> configureOptions)
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
        services.TryAddSingleton<IPiQCorrelationIdFactory, PiQCorrelationIdFactory>();
        services.TryAddScoped<IPiQCorrelationContext, PiQCorrelationContext>();
        services.TryAddTransient<IPiQCorrelationScope, PiQCorrelationScope>();
    }

    /// <summary>
    /// Adds operation-related services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void AddOperationServices(IServiceCollection services)
    {
        services.TryAddSingleton<IPiQOperationContextFactory, PiQOperationContextFactory>();
        services.TryAddScoped<IPiQOperationMetrics, PiQOperationMetrics>();
        services.TryAddScoped<IPiQOperationState, PiQOperationState>();
        services.TryAddTransient<IPiQOperationScope, PiQOperationScope>();
    }
}