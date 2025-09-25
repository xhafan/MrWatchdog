using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogAlertScrapingResultsUpdated;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogAlertScrapingResultsUpdated;

[TestFixture]
public class when_alerting_user_about_new_watchdog_alert_scraping_results : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;
    private WatchdogAlert _watchdogAlert = null!;
    private IEmailSender _emailSender = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _emailSender = A.Fake<IEmailSender>();
        
        var handler = new AlertUserAboutNewWatchdogAlertScrapingResultsDomainEventMessageHandler(
            new NhibernateRepository<WatchdogAlert>(UnitOfWork),
            _emailSender,
            OptionsRetriever.Retrieve<RuntimeOptions>()
        );

        await handler.Handle(new WatchdogAlertScrapingResultsUpdatedDomainEvent(_watchdogAlert.Id));
    }

    [Test]
    public void email_alert_with_new_scraping_results_is_sent_to_user()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _user.Email,
                A<string>.That.Matches(p => p.Contains("new results for the watchdog alert") && p.Contains("Epic Games store free game")),
                A<string>.That.Matches(p => p.Contains("New results have been found for your alert")
                                            && p.Contains($"""
                                                          <a href="https://mrwatchdog_test/Watchdogs/Alert/{_watchdogAlert.Id}">Epic Games store free game</a>
                                                          """)
                                            && p.Contains("""
                                                           <a href="https://store.epicgames.com/en-US/p/machinarium-5e6c71" target="_blank">Machinarium</a>
                                                           """)
                                            && p.Contains("""
                                                          <a href="https://store.epicgames.com/en-US/p/make-way-bddf5f" target="_blank">Make Way</a>
                                                          """)
                )
            ))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void watchdog_alert_scraping_results_to_alert_about_are_cleared()
    {
        _watchdogAlert.ScrapingResultsToAlertAbout.ShouldBeEmpty();
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("Epic Games store free game")
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body h2[id^="what-s-free-on-the-epic-store-this-week"] a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;

        _user = new UserBuilder(UnitOfWork).Build();
        
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(_user)
            .WithSearchTerm(null)
            .Build();
        
        _watchdog.SetScrapingResults(_watchdogWebPageId, [
            "<a href=\"https://store.epicgames.com/en-US/p/machinarium-5e6c71\" target=\"_blank\">Machinarium</a>",
            "<a href=\"https://store.epicgames.com/en-US/p/make-way-bddf5f\" target=\"_blank\">Make Way</a>"
        ]);        
        _watchdog.EnableWebPage(_watchdogWebPageId);

        _watchdogAlert.Refresh();
    }
}