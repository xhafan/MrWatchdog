using FakeItEasy;
using MrWatchdog.Core.Infrastructure.EmailSenders;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.EmailSenderChains;

[TestFixture]
public class when_sending_email_with_one_email_sender_successfully_sending_the_email
{
    private IEmailSender _emailSender = null!;

    [SetUp]
    public async Task Context()
    {
        _emailSender = A.Fake<IEmailSender>();

        var emailSenderChain = new EmailSenderChain(
            [_emailSender]
        );

        await emailSenderChain.SendEmail(
            "xhafan+mrwatchdog_test@gmail.com",
            "test",
            "<span>Test message</span>",
            "https://dev.mrwatchdog.com/api/Watchdogs/123/DisableNotification"
        );
    }

    [Test]
    public void email_is_sent()
    {
        A.CallTo(() => _emailSender.SendEmail(
                "xhafan+mrwatchdog_test@gmail.com",
                "test",
                "<span>Test message</span>",
                "https://dev.mrwatchdog.com/api/Watchdogs/123/DisableNotification"
            ))
            .MustHaveHappenedOnceExactly();
    }
}