// CertApi.Ews.Service/Service/Email/IEwsEmailManagementService.cs
using Microsoft.Exchange.WebServices.Data;
using CertApi.Ews.Core.Interfaces.Context;
using Task = System.Threading.Tasks.Task;

namespace CertApi.Ews.Service.Service.Email
{
    /// <summary>
    /// Interface for managing email messages in Exchange
    /// </summary>
    public interface IEwsEmailManagementService
    {
        /// <summary>
        /// Updates a message
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="message">Message to update</param>
        /// <param name="resolutionMode">Conflict resolution mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated message</returns>
        Task<EmailMessage> UpdateMessageAsync(
            IEwsOperationContext context,
            EmailMessage message,
            ConflictResolutionMode resolutionMode = ConflictResolutionMode.AutoResolve,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="messageId">ID of the message to delete</param>
        /// <param name="deleteMode">Delete mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            DeleteMode deleteMode = DeleteMode.SoftDelete,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves a message to another folder
        /// </summary>
        /// <param name="context">EWS operation context</param>
        /// <param name="messageId">ID of the message to move</param>
        /// <param name="destinationFolderId">Destination folder ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Moved message</returns>
        Task<EmailMessage> MoveMessageAsync(
            IEwsOperationContext context,
            ItemId messageId,
            FolderId destinationFolderId,
            CancellationToken cancellationToken = default);
    }
}
