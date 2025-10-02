using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.WatchDogs.Domain.Events.WatchdogWebPageScrapingFailed;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingFailed;

[TestFixture]
public class when_notifying_user_about_watchdog_scraping_failed_for_watchdog_without_any_web_page_scraping_errors : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private IEmailSender _emailSender = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _emailSender = A.Fake<IEmailSender>();
        
        var handler = new NotifyUserAboutWatchdogScrapingFailedDomainEventMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            _emailSender,
            OptionsTestRetriever.Retrieve<RuntimeOptions>()
        );

        await handler.Handle(new WatchdogWebPageScrapingFailedDomainEvent(_watchdog.Id));
    }

    [Test]
    public void email_notification_about_scraping_errors_is_not_sent_to_user()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _user.Email,
                A<string>._,
                A<string>._
            ))
            .MustNotHaveHappened();
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithUser(_user)
            .Build();
    }
}