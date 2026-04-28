// PiQApi.Ews.Service/Validation/ValidationRuleRegistration.cs
using PiQApi.Abstractions.Validation;
using PiQApi.Ews.Service.Validation.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace PiQApi.Ews.Service.Validation
{
    /// <summary>
    /// Registers validation rules with the validation processor
    /// </summary>
    public static class ValidationRuleRegistration
    {
        /// <summary>
        /// Registers all EWS validation rules with the processor
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        public static void RegisterValidationRules(IServiceProvider serviceProvider)
        {
            var processor = serviceProvider.GetRequiredService<IPiQValidationProcessor>();

            // Register email rules
            processor.RegisterRule(serviceProvider.GetRequiredService<EmailAddressCollectionValidationRule>());
            processor.RegisterRule(serviceProvider.GetRequiredService<MessageBodyValidationRule>());
            processor.RegisterRule(serviceProvider.GetRequiredService<ItemIdValidationRule>());
            processor.RegisterRule(serviceProvider.GetRequiredService<FolderIdValidationRule>());
            processor.RegisterRule(serviceProvider.GetRequiredService<PropertySetValidationRule>());
            processor.RegisterRule(serviceProvider.GetRequiredService<SearchFilterValidationRule>());
        }
    }
}