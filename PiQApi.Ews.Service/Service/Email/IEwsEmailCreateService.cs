// PiQApi.Ews.Service/Service/Email/IEwsEmailCreateService.cs
using Microsoft.Exchange.WebServices.Data;
using PiQApi.Ews.Core.Interfaces.Context;
using Task = System.Threading.Tasks.Task;

namespace PiQApi.Ews.Service.Service.Email
{
    /// <summary>
    /// Interface for creating email messages in Exchange
    /// </summary>
    public interface IEwsEmailCreateService
    {
        /// <summary>
        /// Creates a new email message
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New email message</returns>
        Task<EmailMessage> CreateMessageAsync(IEwsOperationContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new email message with a specific property set
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="propertySet">Property set to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New email message</returns>
        Task<EmailMessage> CreateMessageWithPropertySetAsync(IEwsOperationContext context, PropertySet propertySet, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a message as a draft
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="message">Message to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SaveDraftAsync(IEwsOperationContext context, EmailMessage message, CancellationToken cancellationToken = default);
    }
}
