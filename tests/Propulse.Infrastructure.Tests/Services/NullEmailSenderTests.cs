using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Propulse.Infrastructure.Events;
using Propulse.Infrastructure.Services;

namespace Propulse.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="NullEmailSender{TUser}"/> class.
/// </summary>
public class NullEmailSenderTests
{
    private readonly FakeLogger<NullEmailSender<TestUser>> logger = new();

    [Fact]
    public void DefaultConstructor_CreatesInstanceWithNullLogger()
    {
        var emailSender = new TestableNullEmailSender();

        emailSender.Should().NotBeNull("instance should be created successfully");
        emailSender.Logger.Should().NotBeNull("logger should be initialized");
        emailSender.Logger.GetType().Name.Should().Be("NullLogger", "should use NullLogger by default");
    }

    [Fact]
    public void ParameterizedConstructor_ValidLogger_CreatesInstanceCorrectly()
    {
        var emailSender = new NullEmailSender<TestUser>(logger);

        emailSender.Should().NotBeNull("instance should be created successfully");
    }

    [Fact]
    public void ParameterizedConstructor_NullLogger_ThrowsArgumentNullException()
    {
        var action = () => new NullEmailSender<TestUser>(null!);

        action.Should().Throw<ArgumentNullException>("null logger should not be accepted");
    }

    [Fact]
    public async Task SendAccountConfirmationLinkAsync_ValidParameters_CompletesSuccessfully()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/confirm?token=abc123";
        var emailSender = new NullEmailSender<TestUser>(logger);

        var result = emailSender.SendAccountConfirmationLinkAsync(email, link);

        result.Should().NotBeNull("task should be returned");
        result.IsCompleted.Should().BeTrue("task should complete immediately");
        await result; // Should not throw
    }

    [Fact]
    public async Task SendAccountConfirmationLinkAsync_ValidParameters_RaisesEmailSentEvent()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/confirm?token=abc123";
        var emailSender = new NullEmailSender<TestUser>(logger);
        EmailSenderEventArgs? capturedArgs = null;

        emailSender.EmailSent += (sender, args) => capturedArgs = args;

        await emailSender.SendAccountConfirmationLinkAsync(email, link);

        capturedArgs.Should().NotBeNull("event should be raised");
        capturedArgs!.EventType.Should().Be(EmailSenderEventType.AccountConfirmationLink, "event type should match operation");
        capturedArgs.Email.Should().Be(email, "email should match provided value");
        capturedArgs.LinkOrCode.Should().Be(link, "link should match provided value");
    }

    [Fact]
    public async Task SendAccountConfirmationLinkAsync_ValidParameters_LogsInformation()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/confirm?token=abc123";
        var emailSender = new NullEmailSender<TestUser>(logger);

        await emailSender.SendAccountConfirmationLinkAsync(email, link);

        logger.Collector.GetSnapshot().Should().Contain(x =>
            x.Level == LogLevel.Information && x.Message.Contains(email) && x.Message.Contains(link));
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_ValidParameters_CompletesSuccessfully()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/reset?token=xyz789";
        var emailSender = new NullEmailSender<TestUser>(logger);

        var result = emailSender.SendPasswordResetLinkAsync(email, link);

        result.Should().NotBeNull("task should be returned");
        result.IsCompleted.Should().BeTrue("task should complete immediately");
        await result; // Should not throw
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_ValidParameters_RaisesEmailSentEvent()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/reset?token=xyz789";
        var emailSender = new NullEmailSender<TestUser>(logger);
        EmailSenderEventArgs? capturedArgs = null;

        emailSender.EmailSent += (sender, args) => capturedArgs = args;

        await emailSender.SendPasswordResetLinkAsync(email, link);

        capturedArgs.Should().NotBeNull("event should be raised");
        capturedArgs!.EventType.Should().Be(EmailSenderEventType.PasswordResetLink, "event type should match operation");
        capturedArgs.Email.Should().Be(email, "email should match provided value");
        capturedArgs.LinkOrCode.Should().Be(link, "link should match provided value");
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_ValidParameters_LogsInformation()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/reset?token=xyz789";
        var emailSender = new NullEmailSender<TestUser>(logger);

        await emailSender.SendPasswordResetLinkAsync(email, link);

        logger.Collector.GetSnapshot().Should().Contain(x =>
            x.Level == LogLevel.Information && x.Message.Contains(email) && x.Message.Contains(link));
    }

    [Theory]
    [InlineData("", "https://example.com/confirm")]
    [InlineData("test@example.com", "")]
    [InlineData("", "")]
    public async Task SendAccountConfirmationLinkAsync_EmptyParameters_ThrowsArgumentException(string email, string link)
    {
        var emailSender = new NullEmailSender<TestUser>(logger);

        Func<Task> act = async () => await emailSender.SendAccountConfirmationLinkAsync(email, link);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("", "https://example.com/reset")]
    [InlineData("test@example.com", "")]
    [InlineData("", "")]
    public async Task SendPasswordResetLinkAsync_EmptyParameters_ThrowsArgumentException(string email, string link)
    {
        var emailSender = new NullEmailSender<TestUser>(logger);

        Func<Task> act = async () => await emailSender.SendPasswordResetLinkAsync(email, link);

        await act.Should().ThrowAsync<ArgumentException>();

    }

    [Fact]
    public async Task EmailSentEvent_MultipleSubscribers_AllReceiveNotifications()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/confirm";
        var emailSender = new NullEmailSender<TestUser>(logger);
        var eventCount1 = 0;
        var eventCount2 = 0;

        emailSender.EmailSent += (sender, args) => eventCount1++;
        emailSender.EmailSent += (sender, args) => eventCount2++;

        await emailSender.SendAccountConfirmationLinkAsync(email, link);

        eventCount1.Should().Be(1, "first subscriber should receive the event");
        eventCount2.Should().Be(1, "second subscriber should receive the event");
    }

    [Fact]
    public async Task EmailSentEvent_NoSubscribers_DoesNotThrow()
    {
        const string email = "test@example.com";
        const string link = "https://example.com/confirm";
        var emailSender = new NullEmailSender<TestUser>(logger);

        var action = async () => await emailSender.SendAccountConfirmationLinkAsync(email, link);

        await action.Should().NotThrowAsync("should not throw when no event subscribers exist");
    }

    [Fact]
    public async Task SendAccountConfirmationLinkAsync_CalledMultipleTimes_EachCallLogsAndRaisesEvent()
    {
        const string email1 = "test1@example.com";
        const string link1 = "https://example.com/confirm1";
        const string email2 = "test2@example.com";
        const string link2 = "https://example.com/confirm2";
        var emailSender = new NullEmailSender<TestUser>(logger);
        var eventCount = 0;

        emailSender.EmailSent += (sender, args) => eventCount++;

        await emailSender.SendAccountConfirmationLinkAsync(email1, link1);
        await emailSender.SendAccountConfirmationLinkAsync(email2, link2);

        eventCount.Should().Be(2, "each call should raise an event");
        logger.Collector.GetSnapshot().Should().HaveCount(2);
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_CalledMultipleTimes_EachCallLogsAndRaisesEvent()
    {
        const string email1 = "test1@example.com";
        const string link1 = "https://example.com/reset1";
        const string email2 = "test2@example.com";
        const string link2 = "https://example.com/reset2";
        var emailSender = new NullEmailSender<TestUser>(logger);
        var eventCount = 0;

        emailSender.EmailSent += (sender, args) => eventCount++;

        await emailSender.SendPasswordResetLinkAsync(email1, link1);
        await emailSender.SendPasswordResetLinkAsync(email2, link2);

        eventCount.Should().Be(2, "each call should raise an event");
        logger.Collector.GetSnapshot().Should().HaveCount(2);
    }

    [Fact]
    public async Task MixedEmailOperations_AllOperationsCompleteSuccessfully()
    {
        const string email = "test@example.com";
        const string confirmLink = "https://example.com/confirm";
        const string resetLink = "https://example.com/reset";
        var emailSender = new NullEmailSender<TestUser>(logger);
        var confirmationEventReceived = false;
        var resetEventReceived = false;

        emailSender.EmailSent += (sender, args) =>
        {
            if (args.EventType == EmailSenderEventType.AccountConfirmationLink)
            {
                confirmationEventReceived = true;
            }
            else if (args.EventType == EmailSenderEventType.PasswordResetLink)
            {
                resetEventReceived = true;
            }
        };

        await emailSender.SendAccountConfirmationLinkAsync(email, confirmLink);
        await emailSender.SendPasswordResetLinkAsync(email, resetLink);

        confirmationEventReceived.Should().BeTrue("confirmation event should be received");
        resetEventReceived.Should().BeTrue("reset event should be received");
        logger.Collector.GetSnapshot().Should().HaveCount(2);
    }

    /// <summary>
    /// Test user class for generic type parameter.
    /// </summary>
    private class TestUser
    {
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Testable version of NullEmailSender that exposes the protected Logger property.
    /// </summary>
    private class TestableNullEmailSender : NullEmailSender<TestUser>
    {
        public new ILogger Logger => base.Logger;
    }
}