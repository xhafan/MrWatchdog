using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_persisting_watchdog : BaseDatabaseTest
{
    private Watchdog _newWatchdog = null!;
    private Watchdog? _persistedWatchdog;
    private Scraper _scraper = null!;
    private User _user = null!;

    [SetUp]
    public void Context()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingResults(scraperWebPageId, ["<div>text</div>", "<div>hello</div>"]);
        _scraper.EnableWebPage(scraperWebPageId);

        _user = new UserBuilder(UnitOfWork).Build();
        
        _newWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(_user)
            .WithSearchTerm("text")
            .Build();
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedWatchdog = UnitOfWork.Get<Watchdog>(_newWatchdog.Id);
    }

    [Test]
    public void persisted_watchdog_can_be_retrieved_and_has_correct_data()
    {
        _persistedWatchdog.ShouldNotBeNull();
        _persistedWatchdog.ShouldBe(_newWatchdog);

        _persistedWatchdog.Scraper.ShouldBe(_scraper);
        _persistedWatchdog.User.ShouldBe(_user);
        _persistedWatchdog.SearchTerm.ShouldBe("text");
        _persistedWatchdog.CurrentScrapingResults.ShouldBe(["<div>text</div>"]);
        _persistedWatchdog.IsArchived.ShouldBe(false);
    }
}