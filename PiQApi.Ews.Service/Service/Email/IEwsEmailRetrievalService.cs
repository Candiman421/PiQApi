// PiQApi.Ews.Service/Service/Email/IEwsEmailRetrievalService.cs
using Microsoft.Exchange.WebServices.Data;
using PiQApi.Ews.Core.Interfaces.Context;

namespace PiQApi.Ews.Service.Service.Email
{
    /// <summary>
    /// Interface for retrieving email messages from Exchange
    /// </summary>
    public interface IEwsEmailRetrievalService
    {
        /// <summary>
        /// Gets a message by ID
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="messageId">ID of the message to retrieve</param>
        /// <param name="propertySet">Optional property set to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Email message</returns>
        Task<EmailMessage> GetMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a message with attachments
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="messageId">ID of the message to retrieve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Email message with attachments</returns>
        Task<EmailMessage> GetMessageWithAttachmentsAsync(
            IEwsOperationContext context,
            ItemId messageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds messages in a folder that match the specified filter
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="folderName">Folder to search in</param>
        /// <param name="searchFilter">Optional search filter</param>
        /// <param name="pageSize">Maximum number of items to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Results containing the found items</returns>
        Task<FindItemsResults<Item>> FindMessagesAsync(
            IEwsOperationContext context,
            WellKnownFolderName folderName,
            SearchFilter? searchFilter = null,
            int pageSize = 100,
            CancellationToken cancellationToken = default);
    }
}
