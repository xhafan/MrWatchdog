using FakeItEasy;
using Microsoft.Extensions.Hosting;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.SendEmailCommandMessageHandlers;

[TestFixture]
public class when_sending_email_in_production_environment : BaseDatabaseTest
{
    private IEmailSender _emailSender = null!;

    [SetUp]
    public async Task Context()
    {
        _emailSender = A.Fake<IEmailSender>();

        var hostEnvironment = A.Fake<IHostEnvironment>();
        A.CallTo(() => hostEnvironment.EnvironmentName).Returns(Environments.Production);

        var handler = new SendEmailCommandMessageHandler(new EmailSenderChain([_emailSender]), hostEnvironment);

        await handler.Handle(new SendEmailCommand(
            "recipient@email.com",
            "subject",
            "html message",
            "https://dev.mrwatchdog.com/api/Watchdogs/123/DisableNotification"
        ));
    }

    [Test]
    public void login_link_email_is_sent()
    {
        A.CallTo(() => _emailSender.SendEmail(
                "recipient@email.com",
                "subject",
                "html message",
                "https://dev.mrwatchdog.com/api/Watchdogs/123/DisableNotification"
            ))
            .MustHaveHappenedOnceExactly();
    }


}