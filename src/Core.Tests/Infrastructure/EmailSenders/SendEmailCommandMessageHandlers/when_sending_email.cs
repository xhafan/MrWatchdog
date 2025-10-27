using FakeItEasy;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.SendEmailCommandMessageHandlers;

[TestFixture]
public class when_sending_email : BaseDatabaseTest
{
    private IEmailSender _emailSender = null!;

    [SetUp]
    public async Task Context()
    {
        _emailSender = A.Fake<IEmailSender>();
        var handler = new SendEmailCommandMessageHandler(_emailSender);

        await handler.Handle(new SendEmailCommand("recipient@email.com", "subject", "html message"));
    }

    [Test]
    public void login_link_email_is_sent()
    {
        A.CallTo(() => _emailSender.SendEmail(
                "recipient@email.com",
                "subject",
                "html message"
            ))
            .MustHaveHappenedOnceExactly();
    }
}