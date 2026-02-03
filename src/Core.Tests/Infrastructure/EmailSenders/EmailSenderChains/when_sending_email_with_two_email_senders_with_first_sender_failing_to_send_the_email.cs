using FakeItEasy;
using MrWatchdog.Core.Infrastructure.EmailSenders;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.EmailSenderChains;

[TestFixture]
public class when_sending_email_with_two_email_senders_with_first_sender_failing_to_send_the_email
{
    private IEmailSender _successfulEmailSender = null!;
    private bool _failingEmailSenderTriedToSendEmail;

    [SetUp]
    public async Task Context()
    {
        _successfulEmailSender = A.Fake<IEmailSender>();
        A.CallTo(() => _successfulEmailSender.Priority).Returns(20);

        var emailSenderChain = new EmailSenderChain(
            [
                _successfulEmailSender,
                new FailingFakeEmailSender(() => _failingEmailSenderTriedToSendEmail = true)
            ]
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
        A.CallTo(() => _successfulEmailSender.SendEmail(
                "xhafan+mrwatchdog_test@gmail.com",
                "test",
                "<span>Test message</span>",
                "https://dev.mrwatchdog.com/api/Watchdogs/123/DisableNotification"
            ))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void failing_email_sender_tried_to_send_email()
    {
        _failingEmailSenderTriedToSendEmail.ShouldBe(true);
    }

    private class FailingFakeEmailSender(Action onSendEmailExecuted) : IEmailSender
    {
        public int Priority => 10;

        public Task SendEmail(
            string recipientEmail, 
            string subject, 
            string htmlMessage, 
            string? unsubscribeUrl = null
        )
        {
            onSendEmailExecuted();

            throw new NotImplementedException();
        }
    }
}