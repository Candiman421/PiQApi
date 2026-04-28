// PiQApi.Ews.Service/DependencyInjection/EwsServiceCollectionExtensions.cs
using PiQApi.Ews.Core.Interfaces;
using PiQApi.Ews.Service.Core;
using PiQApi.Ews.Service.Core.Interfaces;
using PiQApi.Ews.Service.Service.Email;
using PiQApi.Ews.Service.Validation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PiQApi.Ews.Service.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering EWS services
    /// </summary>
    public static class EwsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all EWS services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddEwsServices(this IServiceCollection services)
        {
            // Add core services
            services.AddEwsCoreServices();

            // Add email services
            services.AddEwsEmailServices();

            return services;
        }

        /// <summary>
        /// Adds EWS core services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddEwsCoreServices(this IServiceCollection services)
        {
            // Add core services
            services.TryAddSingleton<IEwsErrorMappingService, EwsErrorMappingService>();
            services.TryAddSingleton<IEwsPolicyExecutor, EwsPolicyExecutor>();
            services.TryAddSingleton<IEwsPolicyTypeMapper, EwsPolicyTypeMapper>();
            services.TryAddSingleton<IEwsPropertySetService, EwsPropertySetService>();

            // Add validation rules
            services.TryAddSingleton<EmailAddressCollectionValidationRule>();
            services.TryAddSingleton<ItemIdValidationRule>();
            services.TryAddSingleton<FolderIdValidationRule>();
            services.TryAddSingleton<MessageBodyValidationRule>();
            services.TryAddSingleton<PropertySetValidationRule>();
            services.TryAddSingleton<SearchFilterValidationRule>();

            return services;
        }

        /// <summary>
        /// Adds EWS email services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddEwsEmailServices(this IServiceCollection services)
        {
            // Add email services
            services.TryAddScoped<IEwsEmailCreateService, EwsEmailCreateService>();
            services.TryAddScoped<IEwsEmailRetrievalService, EwsEmailRetrievalService>();
            services.TryAddScoped<IEwsEmailManagementService, EwsEmailManagementService>();
            services.TryAddScoped<IEwsEmailCommunicationService, EwsEmailCommunicationService>();

            return services;
        }
    }
}
