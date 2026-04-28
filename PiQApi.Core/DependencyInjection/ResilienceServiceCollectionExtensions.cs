// PiQApi.Core/DependencyInjection/ResilienceServiceCollectionExtensions.cs
using PiQApi.Abstractions.Resilience;
using PiQApi.Core.Resilience;
using PiQApi.Core.Resilience.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace PiQApi.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering resilience services
/// </summary>
public static class ResilienceServiceCollectionExtensions
{
    /// <summary>
    /// Adds core resilience services to the service collection
    /// </summary>
    public static IServiceCollection AddCoreResilienceServices(this IServiceCollection services)
    {
        // Register policy factory and executor
        _ = services.AddSingleton<IPiQPolicyFactory, PiQPolicyFactory>();
        _ = services.AddSingleton<IPiQResiliencePolicyExecutor, PiQResiliencePolicyExecutor>();

        // Register policy options
        _ = services.AddOptions<PiQPolicyOptions>();

        return services;
    }
}