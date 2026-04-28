// PiQApi.Ews.Operations/Folders/Models/EmailImportInfo.cs

using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;

namespace PiQApi.Ews.Operations.Folders.Models
{
    /// <summary>
    /// Contains information about an email to be imported
    /// </summary>
    public class EmailImportInfo
    {
        /// <summary>
        /// Gets or sets the subject of the email
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the body of the email
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the body type of the email
        /// </summary>
        public BodyType BodyType { get; set; } = BodyType.Text;

        /// <summary>
        /// Gets or sets the 'To' recipients of the email
        /// </summary>
        public List<string>? ToRecipients { get; set; }

        /// <summary>
        /// Gets or sets the 'Cc' recipients of the email
        /// </summary>
        public List<string>? CcRecipients { get; set; }

        /// <summary>
        /// Gets or sets the 'Bcc' recipients of the email
        /// </summary>
        public List<string>? BccRecipients { get; set; }

        /// <summary>
        /// Gets or sets the from address of the email
        /// </summary>
        public string? FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the importance of the email
        /// </summary>
        public Importance Importance { get; set; } = Importance.Normal;

        /// <summary>
        /// Gets or sets the original sent date of the email
        /// </summary>
        public DateTime? OriginalSentDate { get; set; }

        /// <summary>
        /// Gets or sets the original received date of the email
        /// </summary>
        public DateTime? OriginalReceivedDate { get; set; }

        /// <summary>
        /// Gets or sets the categories of the email
        /// </summary>
        public List<string>? Categories { get; set; }

        /// <summary>
        /// Gets or sets the attachments of the email
        /// </summary>
        public List<AttachmentInfo>? Attachments { get; set; }

        /// <summary>
        /// Gets or sets extended properties of the email
        /// </summary>
        public Dictionary<string, string>? ExtendedProperties { get; set; }
    }
}