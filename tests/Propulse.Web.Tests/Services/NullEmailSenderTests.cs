using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Propulse.Web.Entities;
using Propulse.Web.Events;
using Propulse.Web.Services;

namespace Propulse.Web.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="NullEmailSender{TUser}"/> class.
/// </summary>
public class NullEmailSenderTests
{
    private readonly FakeLogger<NullEmailSender<ApplicationUser>> _logger;
    private readonly NullEmailSender<ApplicationUser> _emailSender;
    private readonly ApplicationUser _testUser;

    public NullEmailSenderTests()
    {
        _logger = new FakeLogger<NullEmailSender<ApplicationUser>>();
        _emailSender = new NullEmailSender<ApplicationUser>(_logger);
        _testUser = new ApplicationUser("john.doe@example.com");
    }

    [Fact]
    public void Constructor_ValidLogger_CreatesInstance()
    {
        var logger = new FakeLogger<NullEmailSender<ApplicationUser>>();
        var sender = new NullEmailSender<ApplicationUser>(logger);

        sender.Should().NotBeNull("constructor should create a valid instance");
    }

    [Fact]
    public async Task SendConfirmationLinkAsync_ValidParameters_LogsInformation()
    {
        const string email = "test@example.com";
        const string confirmationLink = "https://example.com/confirm?token=abc123";

        await _emailSender.SendConfirmationLinkAsync(_testUser, email, confirmationLink);

        var logEntry = _logger.Collector.GetSnapshot().Single();
        logEntry.Level.Should().Be(LogLevel.Information, "should log at Information level");
        logEntry.Message.Should().Contain("Account Confirmation Link Sent", "should contain expected message");
        logEntry.Message.Should().Contain(email, "should contain the email address");
        logEntry.Message.Should().Contain(confirmationLink, "should contain the confirmation link");
    }

    [Fact]
    public async Task SendConfirmationLinkAsync_ValidParameters_RaisesEmailSentEvent()
    {
        const string email = "test@example.com";
        const string confirmationLink = "https://example.com/confirm?token=abc123";
        EmailSenderEventArgs? eventArgs = null;

        _emailSender.EmailSent += (sender, args) => eventArgs = args;

        await _emailSender.SendConfirmationLinkAsync(_testUser, email, confirmationLink);

        eventArgs.Should().NotBeNull("EmailSent event should be raised");
        eventArgs!.EventType.Should().Be(EmailSenderEventType.AccountConfirmationLink, "event type should be AccountConfirmationLink");
        eventArgs.Email.Should().Be(email, "event should contain the correct email");
        eventArgs.LinkOrCode.Should().Be(confirmationLink, "event should contain the confirmation link");
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_ValidParameters_LogsInformation()
    {
        const string email = "test@example.com";
        const string resetCode = "123456";

        await _emailSender.SendPasswordResetCodeAsync(_testUser, email, resetCode);

        var logEntry = _logger.Collector.GetSnapshot().Single();
        logEntry.Level.Should().Be(LogLevel.Information, "should log at Information level");
        logEntry.Message.Should().Contain("Password Reset Code sent", "should contain expected message");
        logEntry.Message.Should().Contain(email, "should contain the email address");
        logEntry.Message.Should().Contain(resetCode, "should contain the reset code");
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_ValidParameters_RaisesEmailSentEvent()
    {
        const string email = "test@example.com";
        const string resetCode = "123456";
        EmailSenderEventArgs? eventArgs = null;

        _emailSender.EmailSent += (sender, args) => eventArgs = args;

        await _emailSender.SendPasswordResetCodeAsync(_testUser, email, resetCode);

        eventArgs.Should().NotBeNull("EmailSent event should be raised");
        eventArgs!.EventType.Should().Be(EmailSenderEventType.PasswordResetCode, "event type should be PasswordResetCode");
        eventArgs.Email.Should().Be(email, "event should contain the correct email");
        eventArgs.LinkOrCode.Should().Be(resetCode, "event should contain the reset code");
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_ValidParameters_LogsInformation()
    {
        const string email = "test@example.com";
        const string resetLink = "https://example.com/reset?token=xyz789";

        await _emailSender.SendPasswordResetLinkAsync(_testUser, email, resetLink);

        var logEntry = _logger.Collector.GetSnapshot().Single();
        logEntry.Level.Should().Be(LogLevel.Information, "should log at Information level");
        logEntry.Message.Should().Contain("Password Reset Link sent", "should contain expected message");
        logEntry.Message.Should().Contain(email, "should contain the email address");
        logEntry.Message.Should().Contain(resetLink, "should contain the reset link");
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_ValidParameters_RaisesEmailSentEvent()
    {
        const string email = "test@example.com";
        const string resetLink = "https://example.com/reset?token=xyz789";
        EmailSenderEventArgs? eventArgs = null;

        _emailSender.EmailSent += (sender, args) => eventArgs = args;

        await _emailSender.SendPasswordResetLinkAsync(_testUser, email, resetLink);

        eventArgs.Should().NotBeNull("EmailSent event should be raised");
        eventArgs!.EventType.Should().Be(EmailSenderEventType.PasswordResetLink, "event type should be PasswordResetLink");
        eventArgs.Email.Should().Be(email, "event should contain the correct email");
        eventArgs.LinkOrCode.Should().Be(resetLink, "event should contain the reset link");
    }

    [Fact]
    public async Task EmailSentEvent_NoSubscribers_DoesNotThrow()
    {
        const string email = "test@example.com";
        const string confirmationLink = "https://example.com/confirm?token=abc123";

        var action = async () => await _emailSender.SendConfirmationLinkAsync(_testUser, email, confirmationLink);

        await action.Should().NotThrowAsync("method should not throw when no event subscribers exist");
    }

    [Fact]
    public async Task EmailSentEvent_MultipleSubscribers_NotifiesAllSubscribers()
    {
        const string email = "test@example.com";
        const string confirmationLink = "https://example.com/confirm?token=abc123";
        var subscriber1Called = false;
        var subscriber2Called = false;
        EmailSenderEventArgs? eventArgs1 = null;
        EmailSenderEventArgs? eventArgs2 = null;

        _emailSender.EmailSent += (sender, args) =>
        {
            subscriber1Called = true;
            eventArgs1 = args;
        };

        _emailSender.EmailSent += (sender, args) =>
        {
            subscriber2Called = true;
            eventArgs2 = args;
        };

        await _emailSender.SendConfirmationLinkAsync(_testUser, email, confirmationLink);

        subscriber1Called.Should().BeTrue("first subscriber should be called");
        subscriber2Called.Should().BeTrue("second subscriber should be called");
        eventArgs1.Should().NotBeNull("first subscriber should receive event args");
        eventArgs2.Should().NotBeNull("second subscriber should receive event args");
        eventArgs1!.EventType.Should().Be(eventArgs2!.EventType, "both subscribers should receive same event type");
        eventArgs1.Email.Should().Be(eventArgs2.Email, "both subscribers should receive same email");
        eventArgs1.LinkOrCode.Should().Be(eventArgs2.LinkOrCode, "both subscribers should receive same link or code");
    }

    [Fact]
    public async Task AllMethods_CalledSequentially_LogsAllMessages()
    {
        const string email = "test@example.com";
        const string confirmationLink = "https://example.com/confirm";
        const string resetCode = "123456";
        const string resetLink = "https://example.com/reset";

        await _emailSender.SendConfirmationLinkAsync(_testUser, email, confirmationLink);
        await _emailSender.SendPasswordResetCodeAsync(_testUser, email, resetCode);
        await _emailSender.SendPasswordResetLinkAsync(_testUser, email, resetLink);

        var logEntries = _logger.Collector.GetSnapshot();
        logEntries.Should().HaveCount(3, "should log all three method calls");
        logEntries[0].Message.Should().Contain("Account Confirmation Link", "first log should be for confirmation link");
        logEntries[1].Message.Should().Contain("Password Reset Code", "second log should be for reset code");
        logEntries[2].Message.Should().Contain("Password Reset Link", "third log should be for reset link");
    }

    [Fact]
    public async Task AllMethods_CalledSequentially_RaisesAllEvents()
    {
        const string email = "test@example.com";
        const string confirmationLink = "https://example.com/confirm";
        const string resetCode = "123456";
        const string resetLink = "https://example.com/reset";
        var raisedEvents = new List<EmailSenderEventArgs>();

        _emailSender.EmailSent += (sender, args) => raisedEvents.Add(args);

        await _emailSender.SendConfirmationLinkAsync(_testUser, email, confirmationLink);
        await _emailSender.SendPasswordResetCodeAsync(_testUser, email, resetCode);
        await _emailSender.SendPasswordResetLinkAsync(_testUser, email, resetLink);

        raisedEvents.Should().HaveCount(3, "should raise all three events");
        raisedEvents[0].EventType.Should().Be(EmailSenderEventType.AccountConfirmationLink, "first event should be confirmation link");
        raisedEvents[1].EventType.Should().Be(EmailSenderEventType.PasswordResetCode, "second event should be reset code");
        raisedEvents[2].EventType.Should().Be(EmailSenderEventType.PasswordResetLink, "third event should be reset link");
    }
}
