// CertApi.Core/DependencyInjection/ResilienceServiceCollectionExtensions.cs
using CertApi.Abstractions.Resilience;
using CertApi.Core.Resilience;
using CertApi.Core.Resilience.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CertApi.Core.DependencyInjection;

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
        _ = services.AddSingleton<ICertPolicyFactory, CertPolicyFactory>();
        _ = services.AddSingleton<ICertResiliencePolicyExecutor, CertResiliencePolicyExecutor>();

        // Register policy options
        _ = services.AddOptions<CertPolicyOptions>();

        return services;
    }
}