using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Propulse.Infrastructure.Events;

namespace Propulse.Infrastructure.Services;

/// <summary>
/// Provides a null object implementation of <see cref="IPropulseEmailSender{TUser}"/> that logs email operations without actually sending emails.
/// </summary>
/// <typeparam name="TUser">The type of the user record that emails are being sent for.</typeparam>
/// <remarks>
/// This implementation follows the Null Object pattern to provide a safe default email sender that can be used
/// in development, testing, or scenarios where actual email sending is not required or desired. Instead of sending
/// real emails, it logs the email operations and raises events that can be observed for testing purposes.
/// 
/// <para>
/// The class provides two constructors: a protected parameterless constructor for derived classes that uses a
/// null logger, and a public constructor that accepts an <see cref="ILogger{TCategoryName}"/> for dependency injection.
/// </para>
/// 
/// <para>
/// All email operations are logged at the Information level with structured logging that includes the recipient
/// email address and the link or code being sent. Additionally, an <see cref="EmailSent"/> event is raised for
/// each operation, allowing consumers to observe email sending behavior without relying on actual email delivery.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register the null email sender in dependency injection
/// services.AddScoped&lt;IPropulseEmailSender&lt;ApplicationUser&gt;, NullEmailSender&lt;ApplicationUser&gt;&gt;();
/// 
/// // Usage with dependency injection
/// public class AccountService(IPropulseEmailSender&lt;ApplicationUser&gt; emailSender)
/// {
///     public async Task SendWelcomeEmailAsync(ApplicationUser user)
///     {
///         await emailSender.SendAccountConfirmationLinkAsync(user.Email, "https://example.com/confirm");
///     }
/// }
/// 
/// // Subscribing to email events for testing
/// var emailSender = new NullEmailSender&lt;ApplicationUser&gt;(logger);
/// emailSender.EmailSent += (sender, args) =&gt; 
/// {
///     Console.WriteLine($"Email sent to {args.Email} with link {args.LinkOrCode}");
/// };
/// </code>
/// </example>
/// <seealso cref="IPropulseEmailSender{TUser}"/>
/// <seealso cref="EmailSenderEventArgs"/>
/// <seealso cref="EmailSenderEventType"/>
public class NullEmailSender<TUser> : IPropulseEmailSender<TUser> where TUser : class
{
    /// <summary>
    /// Occurs when an email operation is completed, providing details about the email that was "sent".
    /// </summary>
    /// <remarks>
    /// This event is raised after each email operation completes, allowing subscribers to observe
    /// email sending behavior for testing, monitoring, or auditing purposes. The event arguments
    /// include the type of email operation, recipient address, and the link or code that was sent.
    /// </remarks>
    public event EventHandler<EmailSenderEventArgs>? EmailSent;

    /// <summary>
    /// Initializes a new instance of the <see cref="NullEmailSender{TUser}"/> class for derived classes.
    /// </summary>
    /// <remarks>
    /// This constructor is protected and intended for use by derived classes. It initializes the logger
    /// to <see cref="NullLogger.Instance"/>, which discards all log messages. Derived classes can override
    /// the <see cref="Logger"/> property if they need different logging behavior.
    /// </remarks>
    protected NullEmailSender()
    {
        Logger = NullLogger.Instance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NullEmailSender{TUser}"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging email operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    /// <remarks>
    /// This constructor is intended for use with dependency injection. The provided logger will be used
    /// to log information about email operations at the Information level.
    /// </remarks>
    public NullEmailSender(ILogger<NullEmailSender<TUser>> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    /// <summary>
    /// Gets the logger instance used for logging email operations.
    /// </summary>
    /// <value>
    /// The logger instance. Defaults to <see cref="NullLogger.Instance"/> if not provided through the constructor.
    /// </value>
    /// <remarks>
    /// This property is marked as internal to allow access from test classes while keeping it hidden from
    /// external consumers. The logger is used to record email operations at the Information level.
    /// </remarks>
    internal ILogger Logger { get; }

    /// <summary>
    /// Simulates sending an account confirmation link to the specified email address.
    /// </summary>
    /// <param name="email">The email address to send the confirmation link to.</param>
    /// <param name="link">The confirmation link to be sent to the user.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method logs the account confirmation link operation and raises an <see cref="EmailSent"/> event
    /// with <see cref="EmailSenderEventType.AccountConfirmationLink"/> as the event type. No actual email
    /// is sent, making this safe for development and testing scenarios.
    /// 
    /// <para>
    /// The operation completes immediately with <see cref="Task.CompletedTask"/> since no actual I/O operation
    /// is performed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var emailSender = new NullEmailSender&lt;ApplicationUser&gt;(logger);
    /// await emailSender.SendAccountConfirmationLinkAsync("user@example.com", "https://example.com/confirm?token=abc123");
    /// </code>
    /// </example>
    public Task SendAccountConfirmationLinkAsync(string email, string link)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        ArgumentException.ThrowIfNullOrWhiteSpace(link, nameof(link));

        Logger.LogInformation("Account confirmation link sent to {Email}: {Link}", email, link);
        EmailSenderEventArgs args = new(EmailSenderEventType.AccountConfirmationLink, email, link);
        EmailSent?.Invoke(this, args);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Simulates sending a password reset link to the specified email address.
    /// </summary>
    /// <param name="email">The email address to send the password reset link to.</param>
    /// <param name="link">The password reset link to be sent to the user.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method logs the password reset link operation and raises an <see cref="EmailSent"/> event
    /// with <see cref="EmailSenderEventType.PasswordResetLink"/> as the event type. No actual email
    /// is sent, making this safe for development and testing scenarios.
    /// 
    /// <para>
    /// The operation completes immediately with <see cref="Task.CompletedTask"/> since no actual I/O operation
    /// is performed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var emailSender = new NullEmailSender&lt;ApplicationUser&gt;(logger);
    /// await emailSender.SendPasswordResetLinkAsync("user@example.com", "https://example.com/reset?token=xyz789");
    /// </code>
    /// </example>
    public Task SendPasswordResetLinkAsync(string email, string link)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        ArgumentException.ThrowIfNullOrWhiteSpace(link, nameof(link));

        Logger.LogInformation("Password reset link sent to {Email}: {Link}", email, link);
        EmailSenderEventArgs args = new(EmailSenderEventType.PasswordResetLink, email, link);
        EmailSent?.Invoke(this, args);
        return Task.CompletedTask;
    }
}
