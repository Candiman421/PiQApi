// CertApi.Core/DependencyInjection/ValidationServiceCollectionExtensions.cs
using CertApi.Abstractions.Validation;
using CertApi.Core.Validation;
using CertApi.Core.Validation.Diagnostics;
using CertApi.Core.Validation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CertApi.Core.DependencyInjection;

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
        _ = services.AddSingleton<ICertValidationProcessor, CertValidationProcessor>();
        _ = services.AddSingleton<ICertValidationRuleFactory, CertValidationRuleFactory>();
        _ = services.AddSingleton<ICertValidationService, CertValidationService>();
        _ = services.AddSingleton<ICertValidationDiagnosticsService, CertValidationDiagnosticsService>();

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
            if (typeof(ICertValidationRule<TEntity>).IsAssignableFrom(ruleType))
            {
                _ = services.AddSingleton(typeof(ICertValidationRule<TEntity>), ruleType);
            }
        }

        return services;
    }
}