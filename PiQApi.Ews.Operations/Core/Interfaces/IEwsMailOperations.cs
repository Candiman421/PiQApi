// PiQApi.Ews.Operations/Core/Interfaces/IEwsMailOperations.cs
using Microsoft.Exchange.WebServices.Data;

namespace PiQApi.Ews.Operations.Core.Interfaces
{
    /// <summary>
    /// Interface for mail operations
    /// </summary>
    public interface IEwsMailOperations : IEwsOperationBase
    {
        /// <summary>
        /// Creates a new email message
        /// </summary>
        /// <param name="propertySet">Optional property set for the message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the created email message</returns>
        Task<IEwsResult<EmailMessage>> CreateEmailAsync(
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a backdated email message with the specified date
        /// </summary>
        /// <param name="clientSubmitTime">Client submit time to set</param>
        /// <param name="receivedTime">Received time to set</param>
        /// <param name="propertySet">Optional property set for the message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the created backdated email message</returns>
        Task<IEwsResult<EmailMessage>> CreateBackdatedEmailAsync(
            DateTime clientSubmitTime,
            DateTime receivedTime,
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an email message as a draft
        /// </summary>
        /// <param name="message">Email message to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        Task<IEwsResult> SaveDraftAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an email message
        /// </summary>
        /// <param name="message">Email message to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        Task<IEwsResult> SendEmailAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an email message by ID
        /// </summary>
        /// <param name="messageId">ID of the message to get</param>
        /// <param name="propertySet">Optional property set for the message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result containing the email message</returns>
        Task<IEwsResult<EmailMessage>> GetEmailAsync(
            ItemId messageId,
            PropertySet? propertySet = null,
            CancellationToken cancellationToken = default);
    }
}