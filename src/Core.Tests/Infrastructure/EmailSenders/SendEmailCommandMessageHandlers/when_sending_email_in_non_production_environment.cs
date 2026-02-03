using FakeItEasy;
using Microsoft.Extensions.Hosting;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.SendEmailCommandMessageHandlers;

[TestFixture]
public class when_sending_email_in_non_production_environment : BaseDatabaseTest
{
    private IEmailSender _emailSender = null!;

    [SetUp]
    public async Task Context()
    {
        _emailSender = A.Fake<IEmailSender>();

        var hostEnvironment = A.Fake<IHostEnvironment>();
        A.CallTo(() => hostEnvironment.EnvironmentName).Returns(Environments.Development);
        
        var handler = new SendEmailCommandMessageHandler(new EmailSenderChain([_emailSender]), hostEnvironment);

        await handler.Handle(new SendEmailCommand("recipient@email.com", "subject", "html message"));
    }

    [Test]
    public void login_link_email_is_sent()
    {
        A.CallTo(() => _emailSender.SendEmail(
                "recipient@email.com",
                "(Development) subject",
                "html message"
            ))
            .MustHaveHappenedOnceExactly();
    }
}