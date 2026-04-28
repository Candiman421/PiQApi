// CertApi.Ews.Service/Validation/Rules/ItemIdValidationRule.cs
using CertApi.Abstractions.Validation.Models;
using CertApi.Ews.Core.Validation;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;

namespace CertApi.Ews.Service.Validation.Rules
{
    /// <summary>
    /// Validation rule for ItemId
    /// </summary>
    public class ItemIdValidationRule : EwsBaseValidationRule<ItemId>
    {
        private readonly ILogger<ItemIdValidationRule> _logger;

        public ItemIdValidationRule(ILogger<ItemIdValidationRule> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override CertValidationResult Validate(
            ItemId entity,
            CertValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            var result = new CertValidationResult(true, context.CorrelationId);

            // Check for empty UniqueId
            if (string.IsNullOrWhiteSpace(entity.UniqueId))
            {
                result.AddError(CreateEwsError("EmptyItemId", "Item ID cannot be empty", "ItemId.UniqueId"));
            }

            // Check UniqueId format if it's not empty
            else if (entity.UniqueId.Length > 512)
            {
                result.AddError(CreateEwsError("InvalidItemIdLength", "Item ID is too long (maximum 512 characters)", "ItemId.UniqueId"));
            }

            // Check for properly formatted change key if present
            if (!string.IsNullOrWhiteSpace(entity.ChangeKey) && entity.ChangeKey.Length > 512)
            {
                result.AddError(CreateEwsError("InvalidChangeKeyLength", "Change key is too long (maximum 512 characters)", "ItemId.ChangeKey"));
            }

            return result;
        }
    }
}