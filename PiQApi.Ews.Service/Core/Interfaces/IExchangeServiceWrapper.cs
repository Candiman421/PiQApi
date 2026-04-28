// PiQApi.Ews.Service/Core/Interfaces/IExchangeServiceWrapper.cs
using PiQApi.Abstractions.Enums;
using PiQApi.Core.Authentication;
using Microsoft.Exchange.WebServices.Data;
using Task = System.Threading.Tasks.Task;

namespace PiQApi.Ews.Service.Core.Interfaces
{
    /// <summary>
    /// Interface for Exchange service wrapper
    /// </summary>
    public interface IExchangeServiceWrapper : IAsyncDisposable
    {
        /// <summary>
        /// Gets whether the service is connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the service URL
        /// </summary>
        Uri ServiceUri { get; }

        /// <summary>
        /// Gets the underlying Exchange service
        /// </summary>
        ExchangeService Service { get; }

        /// <summary>
        /// Gets the authentication status
        /// </summary>
        AuthenticationStatusType AuthenticationStatus { get; }

        /// <summary>
        /// Gets the current correlation ID if set
        /// </summary>
        string CorrelationId { get; }

        // Auth methods
        Task AuthenticateAsync(PiQAuthenticationOptions options, CancellationToken cancellationToken = default);
        Task ConfigureImpersonationAsync(string userId, CancellationToken cancellationToken = default);
        Task ConfigureImpersonationAsync(string userId, ConnectingIdType idType, CancellationToken cancellationToken = default);
        Task ConfigureServiceAsync(string token, string tenantId, CancellationToken cancellationToken = default);
        Task ConfigureServiceUrlAsync(Uri serviceUrl, CancellationToken cancellationToken = default);
        Task ConfigureTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
        Task ConfigureTraceEnabledAsync(bool enabled, CancellationToken cancellationToken = default);
        Task<ExchangeVersion> GetServerVersionAsync(CancellationToken cancellationToken = default);
        Task InvalidateTokenAsync(string token, CancellationToken cancellationToken = default);
        Task SetCredentialsAsync(string credentials, PiQAuthenticationType authType = PiQAuthenticationType.AppOnly, CancellationToken cancellationToken = default);
        Task SetOAuthCredentialsAsync(string token);
        Task ValidateConnectionAsync(CancellationToken cancellationToken = default);
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

        // Service wrapper methods that directly access the EWS API
        Task<ServiceResponseCollection<ServiceResponse>> DeleteItemsAsync(
            IEnumerable<ItemId> itemIds,
            DeleteMode deleteMode,
            SendCancellationsMode sendCancellationsMode,
            AffectedTaskOccurrence affectedTaskOccurrence,
            CancellationToken cancellationToken = default);

        Task<ServiceResponseCollection<MoveCopyItemResponse>> MoveItemsAsync(
            IEnumerable<ItemId> itemIds,
            FolderId destinationFolderId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponseCollection<ItemInfoResponse>> CreateItemsAsync<TItem>(
            IEnumerable<TItem> items,
            FolderId parentFolderId,
            MessageDisposition messageDisposition,
            MessageDispositionSuppression? suppressReadReceipts,
            CancellationToken cancellationToken = default) where TItem : Item;

        /// <summary>
        /// Sets a custom HTTP header
        /// </summary>
        /// <param name="headerName">Header name</param>
        /// <param name="headerValue">Header value</param>
        void SetHeader(string headerName, string headerValue);

        /// <summary>
        /// Sets the correlation ID for tracking requests
        /// </summary>
        /// <param name="correlationId">Correlation ID to set</param>
        void SetCorrelationId(string correlationId);

        void SetImpersonatedUserId(ConnectingIdType idType, string id);

        // Folder methods
        Task<Folder> CreateFolderAsync(string displayName, WellKnownFolderName parentFolderName, CancellationToken cancellationToken = default);
        Task DeleteFolderAsync(FolderId folderId, DeleteMode deleteMode = DeleteMode.SoftDelete, CancellationToken cancellationToken = default);
        Task<Folder> GetDefaultFolderAsync(WellKnownFolderName folderName, CancellationToken cancellationToken = default);
        Task<Folder> GetFolderAsync(FolderId folderId, PropertySet? propertySet = null, CancellationToken cancellationToken = default);
        Task<Folder> MoveFolderAsync(FolderId folderId, FolderId destinationFolderId, CancellationToken cancellationToken = default);
        Task<Folder> UpdateFolderAsync(Folder folder, CancellationToken cancellationToken = default);

        // Mail methods
        Task<EmailMessage> CreateMessageAsync(PropertySet? propertySet = null, CancellationToken cancellationToken = default);
        Task DeleteMessageAsync(ItemId messageId, DeleteMode deleteMode = DeleteMode.SoftDelete, CancellationToken cancellationToken = default);
        Task<EmailMessage> ForwardMessageAsync(ItemId messageId, IEnumerable<string> toRecipients, string comment, CancellationToken cancellationToken = default);
        Task<EmailMessage> GetMessageAsync(ItemId messageId, PropertySet? propertySet = null, CancellationToken cancellationToken = default);
        Task<EmailMessage> GetMessageWithAttachmentsAsync(ItemId messageId, CancellationToken cancellationToken = default);
        Task<EmailMessage> ReplyToMessageAsync(ItemId messageId, string response, bool replyAll = false, CancellationToken cancellationToken = default);
        Task SaveDraftAsync(EmailMessage message, CancellationToken cancellationToken = default);
        Task SendMessageAsync(EmailMessage message, CancellationToken cancellationToken = default);
        Task<EmailMessage> UpdateMessageAsync(EmailMessage message, ConflictResolutionMode resolutionMode = ConflictResolutionMode.AutoResolve, CancellationToken cancellationToken = default);

        // Extended property helpers
        Task SetExtendedPropertyAsync(Item item, ExtendedPropertyDefinition propertyDefinition, object value, CancellationToken cancellationToken = default);
        Task<T?> GetExtendedPropertyValueAsync<T>(Item item, ExtendedPropertyDefinition propertyDefinition, CancellationToken cancellationToken = default);
    }
}
