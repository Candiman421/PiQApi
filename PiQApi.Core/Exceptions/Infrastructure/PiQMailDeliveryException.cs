// PiQApi.Core/Exceptions/Infrastructure/PiQMailDeliveryException.cs
using PiQApi.Core.Exceptions.Base;

namespace PiQApi.Core.Exceptions.Infrastructure;

/// <summary>
/// Exception thrown when an email delivery fails
/// </summary>
public class PiQMailDeliveryException : PiQServiceException
{
    /// <summary>
    /// Gets the email address of the recipient
    /// </summary>
    public string? RecipientEmail { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQMailDeliveryException"/> class
    /// </summary>
    public PiQMailDeliveryException()
        : base("A mail delivery error occurred", "MailDeliveryError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQMailDeliveryException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PiQMailDeliveryException(string message)
        : base(message, "MailDeliveryError")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQMailDeliveryException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQMailDeliveryException(string message, Exception? inner)
        : base(message, "MailDeliveryError", inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQMailDeliveryException"/> class with a specified error message
    /// and recipient email
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="recipientEmail">The email address of the recipient</param>
    public PiQMailDeliveryException(string message, string? recipientEmail)
        : base(message, "MailDeliveryError")
    {
        RecipientEmail = recipientEmail;

        if (!string.IsNullOrEmpty(recipientEmail))
        {
            AddData(nameof(RecipientEmail), recipientEmail);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PiQMailDeliveryException"/> class with a specified error message,
    /// recipient email, and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="recipientEmail">The email address of the recipient</param>
    /// <param name="inner">The exception that is the cause of the current exception</param>
    public PiQMailDeliveryException(string message, string? recipientEmail, Exception? inner)
        : base(message, "MailDeliveryError", inner)
    {
        RecipientEmail = recipientEmail;

        if (!string.IsNullOrEmpty(recipientEmail))
        {
            AddData(nameof(RecipientEmail), recipientEmail);
        }
    }
}