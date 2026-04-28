// CertApi.Ews.Service/Validation/Rules/EmailAddressCollectionValidationRule.cs
using CertApi.Abstractions.Validation.Models;
using CertApi.Ews.Core.Validation;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;

namespace CertApi.Ews.Service.Validation.Rules
{
    /// <summary>
    /// Validation rule for EmailAddressCollection
    /// </summary>
    public class EmailAddressCollectionValidationRule : EwsBaseValidationRule<EmailAddressCollection>
    {
        private readonly int _maxRecipients = 500;
        private readonly ILogger<EmailAddressCollectionValidationRule> _logger;

        public EmailAddressCollectionValidationRule(ILogger<EmailAddressCollectionValidationRule> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override CertValidationResult Validate(
            EmailAddressCollection entity,
            CertValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            var result = new CertValidationResult(true, context.CorrelationId);

            // Check recipient count
            if (entity.Count > _maxRecipients)
            {
                result.AddError(
                    CreateEwsError("TooManyRecipients",
                    $"Too many recipients in collection. Maximum allowed is {_maxRecipients}",
                    "Recipients"));
            }

            // Validate individual email addresses
            for (int i = 0; i < entity.Count; i++)
            {
                var address = entity[i];
                if (address == null)
                {
                    result.AddError(CreateEwsError("NullEmailAddress", "Email address cannot be null", $"EmailAddress[{i}]"));
                    continue;
                }

                // Check for empty address
                if (string.IsNullOrWhiteSpace(address.Address))
                {
                    result.AddError(CreateEwsError("EmptyEmailAddress", "Email address cannot be empty", $"EmailAddress[{i}].Address"));
                }
                else
                {
                    // Basic email format validation
                    if (!IsValidEmailFormat(address.Address))
                    {
                        result.AddError(CreateEwsError("InvalidEmailFormat",
                            $"Invalid email address format: {address.Address}",
                            $"EmailAddress[{i}].Address"));
                    }
                }
            }

            return result;
        }

        private static bool IsValidEmailFormat(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}