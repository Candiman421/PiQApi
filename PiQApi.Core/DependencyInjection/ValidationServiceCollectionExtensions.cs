// PiQApi.Core/DependencyInjection/ValidationServiceCollectionExtensions.cs
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Validation;
using PiQApi.Core.Validation.Diagnostics;
using PiQApi.Core.Validation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PiQApi.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering validation services with DI
/// </summary>
public static class ValidationServiceCollectionExtensions
{
    /// <summary>
    /// Adds validation services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCertValidation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register core validation services
        _ = services.AddSingleton<IPiQValidationProcessor, PiQValidationProcessor>();
        _ = services.AddSingleton<IPiQValidationRuleFactory, PiQValidationRuleFactory>();
        _ = services.AddSingleton<IPiQValidationService, PiQValidationService>();
        _ = services.AddSingleton<IPiQValidationDiagnosticsService, PiQValidationDiagnosticsService>();

        return services;
    }

    /// <summary>
    /// Adds validation rules for a specific entity type
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="rules">Validation rules to register</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddValidationRules<TEntity>(
        this IServiceCollection services,
        params Type[] rules) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(rules);

        foreach (var ruleType in rules)
        {
            if (typeof(IPiQValidationRule<TEntity>).IsAssignableFrom(ruleType))
            {
                _ = services.AddSingleton(typeof(IPiQValidationRule<TEntity>), ruleType);
            }
        }

        return services;
    }
}