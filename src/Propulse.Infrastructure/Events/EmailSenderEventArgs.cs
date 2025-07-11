namespace Propulse.Infrastructure.Events;

/// <summary>
/// The list of potential events that can be raised by the email sender.
/// </summary>
public enum EmailSenderEventType
{
    /// <summary>
    /// An account confirmation link was sent to the user.
    /// </summary>
    AccountConfirmationLink,

    /// <summary>
    /// A password reset link was sent to the user.
    /// </summary>
    PasswordResetLink
};

/// <summary>
/// The event arguments for the email sender events.
/// </summary>
public class EmailSenderEventArgs(EmailSenderEventType eventType, string email, string linkOrCode) : EventArgs
{
    /// <summary>
    /// Gets the type of the email sender event.
    /// </summary>
    public EmailSenderEventType EventType { get; } = eventType;

    /// <summary>
    /// Gets the email address the link or code was sent to.
    /// </summary>
    public string Email { get; } = email;

    /// <summary>
    /// Gets the link or code associated with the current instance.
    /// </summary>
    public string LinkOrCode { get; } = linkOrCode;
}
