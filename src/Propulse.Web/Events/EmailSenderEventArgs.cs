namespace Propulse.Web.Events;

/// <summary>
/// Specifies the type of email event for which an email is being sent.
/// </summary>
/// <remarks>
/// Used to distinguish between different email scenarios such as account confirmation, password reset code, and password reset link.
/// </remarks>
public enum EmailSenderEventType
{
    /// <summary>
    /// Event for sending an account confirmation link.
    /// </summary>
    AccountConfirmationLink,

    /// <summary>
    /// Event for sending a password reset code.
    /// </summary>
    PasswordResetCode,

    /// <summary>
    /// Event for sending a password reset link.
    /// </summary>
    PasswordResetLink
}

/// <summary>
/// Provides data for email sender events, including event type, recipient email, and link or code.
/// </summary>
/// <remarks>
/// This class is used to pass event data when sending emails for account confirmation or password reset scenarios.
/// </remarks>
/// <example>
/// <code>
/// var args = new EmailSenderEventArgs(EmailSenderEventType.AccountConfirmationLink, "user@example.com", "https://example.com/confirm?token=abc");
/// </code>
/// </example>
public class EmailSenderEventArgs(EmailSenderEventType eventType, string email, string linkOrCode) : EventArgs
{
    /// <summary>
    /// Gets the type of email event.
    /// </summary>
    /// <value>The <see cref="EmailSenderEventType"/> indicating the event scenario.</value>
    public EmailSenderEventType EventType { get; } = eventType;

    /// <summary>
    /// Gets the recipient email address.
    /// </summary>
    /// <value>The email address to which the email is sent.</value>
    public string Email { get; } = email;

    /// <summary>
    /// Gets the link or code associated with the email event.
    /// </summary>
    /// <value>The confirmation link or reset code sent to the user.</value>
    public string LinkOrCode { get; } = linkOrCode;
}