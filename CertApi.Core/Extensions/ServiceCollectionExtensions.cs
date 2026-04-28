// CertApi.Core/Extensions/ServiceCollectionExtensions.cs
using CertApi.Abstractions.Factories;
using CertApi.Abstractions.Validation;
using CertApi.Core.Validation;
using CertApi.Core.Validation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CertApi.Core.Extensions;

/// <summary>
/// Extensions for registering CertApi services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core validation services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCertValidation(this IServiceCollection services)
    {
        // Add validation factory and processor
        services.AddSingleton<ICertValidationResultFactory, CertValidationResultFactory>();
        services.AddSingleton<ICertValidationProcessor, CertValidationProcessor>();
        services.AddSingleton<ICertValidationContextFactory, CertValidationContextFactory>();
        services.AddSingleton<ICertValidationService, CertValidationService>();
        services.AddSingleton<ICertValidationRuleFactory, CertValidationRuleFactory>();

        return services;
    }
}