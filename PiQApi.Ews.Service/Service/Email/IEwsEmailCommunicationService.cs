// PiQApi.Ews.Service/Service/Email/IEwsEmailCommunicationService.cs
using PiQApi.Ews.Core.Interfaces.Context;
using Microsoft.Exchange.WebServices.Data;
using Task = System.Threading.Tasks.Task;

namespace PiQApi.Ews.Service.Service.Email
{
    /// <summary>
    /// Interface for email communication operations in Exchange
    /// </summary>
    public interface IEwsEmailCommunicationService
    {
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SendMessageAsync(
            IEwsOperationContext context,
            EmailMessage message,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Forwards a message to recipients
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="messageId">ID of the message to forward</param>
        /// <param name="toRecipients">Recipients to forward to</param>
        /// <param name="comment">Optional comment to include</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Original message that was forwarded</returns>
        Task<EmailMessage> ForwardMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            IEnumerable<string> toRecipients,
            string comment,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Replies to a message
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="messageId">ID of the message to reply to</param>
        /// <param name="response">Response text</param>
        /// <param name="replyAll">Whether to reply to all recipients</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Original message that was replied to</returns>
        Task<EmailMessage> ReplyToMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            string response,
            bool replyAll = false,
            CancellationToken cancellationToken = default);
    }
}
