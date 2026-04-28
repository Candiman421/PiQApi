// PiQApi.Ews.Service/Validation/Rules/MessageBodyValidationRule.cs
using PiQApi.Abstractions.Validation.Models;
using PiQApi.Ews.Core.Validation;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;

namespace PiQApi.Ews.Service.Validation.Rules
{
    /// <summary>
    /// Validation rule for MessageBody
    /// </summary>
    public class MessageBodyValidationRule : EwsBaseValidationRule<MessageBody>
    {
        private readonly int _maxBodyLength = 10 * 1024 * 1024; // 10 MB
        private readonly ILogger<MessageBodyValidationRule> _logger;

        public MessageBodyValidationRule(ILogger<MessageBodyValidationRule> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override PiQValidationResult Validate(
            MessageBody entity,
            PiQValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(context);

            var result = new PiQValidationResult(true, context.CorrelationId);

            // Check body text length
            if (entity.Text.Length > _maxBodyLength)
            {
                result.AddError(CreateEwsError(
                    "MessageBodyTooLarge",
                    $"Message body exceeds maximum length of {_maxBodyLength / (1024 * 1024)} MB",
                    "MessageBody.Text"));
            }

            // Check body type
            if (entity.BodyType != BodyType.HTML && entity.BodyType != BodyType.Text)
            {
                result.AddError(CreateEwsError(
                    "InvalidBodyType",
                    "Invalid body type. Expected HTML or Text",
                    "MessageBody.BodyType"));
            }

            // HTML validation if needed
            if (entity.BodyType == BodyType.HTML)
            {
                if (!IsValidHtml(entity.Text))
                {
                    result.AddError(CreateEwsError(
                        "InvalidHtmlContent",
                        "HTML body contains invalid or potentially unsafe content",
                        "MessageBody.Text"));
                }
            }

            return result;
        }

        private bool IsValidHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return true;
            }

            // Basic HTML validation - check for balanced tags
            // In a real implementation, this would be more robust

            // Check for potentially unsafe content
            string[] unsafePatterns = {
                "<script", "javascript:", "vbscript:",
                "onclick=", "onload=", "onerror=", "onmouseover="
            };

            foreach (var pattern in unsafePatterns)
            {
                if (html.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}