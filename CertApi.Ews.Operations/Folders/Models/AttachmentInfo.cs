// CertApi.Ews.Operations/Folders/Models/AttachmentInfo.cs

namespace CertApi.Ews.Operations.Folders.Models
{
    /// <summary>
    /// Contains information about an attachment
    /// </summary>
    public class AttachmentInfo
    {
        /// <summary>
        /// Gets or sets the name of the attachment
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content of the attachment
        /// </summary>
        public byte[]? Content { get; set; }

        /// <summary>
        /// Gets or sets the content ID of the attachment
        /// </summary>
        public string? ContentId { get; set; }

        /// <summary>
        /// Gets or sets the content type of the attachment
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets whether the attachment is a file attachment
        /// </summary>
        public bool IsFileAttachment { get; set; } = true;
    }
}