// PiQApi.Core/DependencyInjection/CorrelationServiceCollectionExtensions.cs
using PiQApi.Abstractions.Core;
using PiQApi.Abstractions.Factories;
using PiQApi.Core.Core;
using PiQApi.Core.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace PiQApi.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering correlation services
/// </summary>
public static class CorrelationServiceCollectionExtensions
{
    /// <summary>
    /// Adds correlation services to the service collection
    /// </summary>
    public static IServiceCollection AddCorrelationServices(this IServiceCollection services)
    {
        // Register correlation context as scoped to maintain correlation ID within a request
        _ = services.AddScoped<ICertCorrelationContext, CertCorrelationContext>();

        // Register correlation ID factory as singleton
        _ = services.AddSingleton<ICertCorrelationIdFactory, CertCorrelationIdFactory>();

        return services;
    }
}