using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.EmailSenders.Domain;
using MrWatchdog.TestsShared;
using NHibernate.Criterion;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.SmtpClientDirectlyToRecipientMailServerEmailSenders;

[TestFixture]
public class when_sending_email_again_with_the_mail_server_in_cool_down_period_due_to_rate_limiting
{
    private SmtpClientDirectlyToRecipientMailServerEmailSender _emailSender = null!;

    [Test]
    public async Task exception_is_thrown_without_existing_mail_server_rate_limiting_record()
    {
        _emailSender = new SmtpClientDirectlyToRecipientMailServerEmailSender(
            OptionsTestRetriever.Retrieve<SmtpClientDirectlyToRecipientMailServerEmailSenderOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>(),
            TestFixtureContext.NhibernateConfigurator,
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

    [Test]
    public async Task exception_is_thrown_with_existing_mail_server_rate_limiting_record()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                var mailServerRateLimiting = new MailServerRateLimiting("gmail-smtp-in.l.google.com.", DateTime.UtcNow.AddHours(-24));
                await newUnitOfWork.Session!.SaveAsync(mailServerRateLimiting);
            }
        );

        _emailSender = new SmtpClientDirectlyToRecipientMailServerEmailSender(
            OptionsTestRetriever.Retrieve<SmtpClientDirectlyToRecipientMailServerEmailSenderOptions>(),
            OptionsTestRetriever.Retrieve<EmailAddressesOptions>(),
            TestFixtureContext.NhibernateConfigurator,
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


    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                var mailServerRateLimiting = await newUnitOfWork.Session!.QueryOver<MailServerRateLimiting>()
                    .Where(x => x.MailServerName.IsInsensitiveLike("%gmail%"))
                    .SingleOrDefaultAsync();

                if (mailServerRateLimiting != null)
                {
                    await newUnitOfWork.Session!.DeleteAsync(mailServerRateLimiting);
                }
            }
        );
    }
}