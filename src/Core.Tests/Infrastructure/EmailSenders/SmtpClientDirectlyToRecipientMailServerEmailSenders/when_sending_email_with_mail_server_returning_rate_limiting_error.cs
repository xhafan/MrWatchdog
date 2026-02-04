using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.SmtpClientDirectlyToRecipientMailServerEmailSenders;

[TestFixture]
public class when_sending_email_with_mail_server_returning_rate_limiting_error
{
    private SmtpClientDirectlyToRecipientMailServerEmailSender _emailSender = null!;

    [SetUp]
    public void Context()
    {
        _emailSender = new SmtpClientDirectlyToRecipientMailServerEmailSender(
            OptionsTestRetriever.Retrieve<SmtpClientDirectlyToRecipientMailServerEmailSenderOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>(),
            new FakeHybridCache(),
            smtpClientFactory: () => new FakeRateLimitingSmtpClient()
        );
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<EmailSendingRateLimitedException>(async () => await _emailSender.SendEmail(
            "xhafan+mrwatchdog_test@gmail.com",
            "test",
            "<span>Test message</span>"
        ));

        ex.Message.ShouldBe("Email not sent due to rate limiting by gmail-smtp-in.l.google.com. mail server.");
    }
}