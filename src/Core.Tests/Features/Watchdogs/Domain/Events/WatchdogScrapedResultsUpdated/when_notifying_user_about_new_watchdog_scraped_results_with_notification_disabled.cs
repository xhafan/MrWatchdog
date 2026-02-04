using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;

[TestFixture]
public class when_notifying_user_about_new_watchdog_scraped_results_with_notification_disabled : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _bus = A.Fake<ICoreBus>();
        
        var handler = new NotifyUserAboutNewWatchdogScrapedResultsDomainEventMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            _bus,
            OptionsTestRetriever.Retrieve<RuntimeOptions>()
        );

        await handler.Handle(new WatchdogScrapedResultsUpdatedDomainEvent(_watchdog.Id));
    }

    [Test]
    public void email_notification_about_new_scraped_results_is_not_sent_to_user()
    {
        A.CallTo(() => _bus.Send(A<Command>._)).MustNotHaveHappened();
    }

    [Test]
    public void watchdog_scraped_results_to_notify_about_are_cleared()
    {
        _watchdog.ScrapedResultsToNotifyAbout.ShouldBeEmpty();
    }

    [Test]
    public void watchdog_scraped_results_history_is_empty()
    {
        _watchdog.ScrapedResultsHistory.ShouldBeEmpty();
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("Epic Games store free game")
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body h2[id^="what-s-free-on-the-epic-store-this-week"] a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;

        _user = new UserBuilder(UnitOfWork).Build();
        
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(_user)
            .WithReceiveNotification(false)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapedResults(_scraperWebPageId, [
            "<a href=\"https://store.epicgames.com/en-US/p/machinarium-5e6c71\" target=\"_blank\">Machinarium</a>",
            "<a href=\"https://store.epicgames.com/en-US/p/make-way-bddf5f\" target=\"_blank\">Make Way</a>"
        ]);        
        _scraper.EnableWebPage(_scraperWebPageId);

        _watchdog.Refresh();
    }
}