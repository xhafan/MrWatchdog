using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.SmtpClientDirectlyToRecipientMailServerEmailSenders;

[TestFixture]
public class when_sending_email_again_with_the_mail_server_in_cool_down_period_due_to_rate_limiting
{
    private SmtpClientDirectlyToRecipientMailServerEmailSender _emailSender = null!;

    [Test]
    public async Task exception_is_thrown()
    {
        _emailSender = new SmtpClientDirectlyToRecipientMailServerEmailSender(
            OptionsTestRetriever.Retrieve<SmtpClientDirectlyToRecipientMailServerEmailSenderOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>(),
            new FakeHybridCache(),
            smtpClientFactory: () => new FakeRateLimitingSmtpClient()
        );

        try
        {
            await _emailSender.SendEmail(
                "xhafan+mrwatchdog_test@gmail.com",
                "test",
                "<span>Test message</span>"
            );
        }
        catch
        {
            // ignored
        }

        var ex = Should.Throw<EmailSendingNotAllowedInCoolDownPeriodException>(async () => await _emailSender.SendEmail(
            "xhafan+mrwatchdog_test@gmail.com",
            "test",
            "<span>Test message</span>"
        ));

        ex.Message.ShouldBe("Email not sent due to gmail-smtp-in.l.google.com. mail server being in cool down period due to the previous email being rate limited.");
    }
}