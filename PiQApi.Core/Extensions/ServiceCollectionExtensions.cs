// PiQApi.Core/Extensions/ServiceCollectionExtensions.cs
using PiQApi.Abstractions.Factories;
using PiQApi.Abstractions.Validation;
using PiQApi.Core.Validation;
using PiQApi.Core.Validation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PiQApi.Core.Extensions;

/// <summary>
/// Extensions for registering PiQApi services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core validation services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddPiQValidation(this IServiceCollection services)
    {
        // Add validation factory and processor
        services.AddSingleton<IPiQValidationResultFactory, PiQValidationResultFactory>();
        services.AddSingleton<IPiQValidationProcessor, PiQValidationProcessor>();
        services.AddSingleton<IPiQValidationContextFactory, PiQValidationContextFactory>();
        services.AddSingleton<IPiQValidationService, PiQValidationService>();
        services.AddSingleton<IPiQValidationRuleFactory, PiQValidationRuleFactory>();

        return services;
    }
}
