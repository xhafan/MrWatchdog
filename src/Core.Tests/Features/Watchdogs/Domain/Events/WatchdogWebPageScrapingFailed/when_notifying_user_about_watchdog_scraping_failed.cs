using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingFailed;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Core.Infrastructure.Configurations;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingFailed;

[TestFixture]
public class when_notifying_user_about_watchdog_scraping_failed : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;
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
    public void email_notification_about_scraping_errors_is_sent_to_user()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _user.Email,
                A<string>.That.Matches(p => p.Contains("web scraping failed") && p.Contains("Epic Games store free game")),
                A<string>.That.Matches(p => p.Contains("Web scraping failed")
                                            && p.Contains($"""
                                                           <a href="https://mrwatchdog_test/Watchdogs/Detail/{_watchdog.Id}">Epic Games store free game</a>
                                                           """)
                                            && p.Contains($"""
                                                           <a href="https://mrwatchdog_test/Watchdogs/Detail/{_watchdog.Id}#watchdog_web_page_{_watchdogWebPageId}">www.pcgamer.com/epic-games-store-free-games-list/</a>
                                                           """)
                                            && p.Contains("""
                                                           Network error
                                                           """)
                )
            ))
            .MustHaveHappenedOnceExactly();
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .WithName("Epic Games store free game")
            .WithUser(_user)
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingErrorMessage(_watchdogWebPageId, "Network error");
    }
}