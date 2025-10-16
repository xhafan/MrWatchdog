using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders;

[TestFixture]
[Ignore("Email sending is disabled by default")]
public class when_sending_email
{
    [Test]
    public async Task email_is_sent()
    {
        var emailSender = new EmailSender(
            OptionsTestRetriever.Retrieve<EmailSenderOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>()
        );

        await emailSender.SendEmail("xhafan+mrwatchdog_test@gmail.com", "test", "<span>Test message</span>");
    }
}