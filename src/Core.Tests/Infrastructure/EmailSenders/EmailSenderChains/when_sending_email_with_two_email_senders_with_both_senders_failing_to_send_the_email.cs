using MrWatchdog.Core.Infrastructure.EmailSenders;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.EmailSenderChains;

[TestFixture]
public class when_sending_email_with_two_email_senders_with_both_senders_failing_to_send_the_email
{
    private EmailSenderChain _emailSenderChain = null!;

    [SetUp]
    public void Context()
    {
        _emailSenderChain = new EmailSenderChain(
            [
                new FailingFakeEmailSender(),
                new FailingFakeEmailSender()
            ]
        );
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _emailSenderChain.SendEmail(
            "xhafan+mrwatchdog_test@gmail.com",
            "test",
            "<span>Test message</span>",
            "https://dev.mrwatchdog.com/api/Watchdogs/123/DisableNotification"
        ));

        ex.Message.ShouldBe(
            """
            Could not send email:
            FailingFakeEmailSender: Error sending email.
            FailingFakeEmailSender: Error sending email.
            """,
            ignoreLineEndings: true
        );
    }

    private class FailingFakeEmailSender : IEmailSender
    {
        public int Priority => 10;

        public Task SendEmail(
            string recipientEmail, 
            string subject, 
            string htmlMessage, 
            string? unsubscribeUrl = null
        )
        {
            throw new Exception("Error sending email.");
        }
    }
}