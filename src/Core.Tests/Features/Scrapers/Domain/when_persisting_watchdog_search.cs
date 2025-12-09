using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_persisting_watchdog_search : BaseDatabaseTest
{
    private WatchdogSearch _newWatchdogSearch = null!;
    private WatchdogSearch? _persistedWatchdogSearch;
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
        
        _newWatchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(_user)
            .WithSearchTerm("text")
            .Build();
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedWatchdogSearch = UnitOfWork.Get<WatchdogSearch>(_newWatchdogSearch.Id);
    }

    [Test]
    public void persisted_watchdog_search_can_be_retrieved_and_has_correct_data()
    {
        _persistedWatchdogSearch.ShouldNotBeNull();
        _persistedWatchdogSearch.ShouldBe(_newWatchdogSearch);

        _persistedWatchdogSearch.Scraper.ShouldBe(_scraper);
        _persistedWatchdogSearch.User.ShouldBe(_user);
        _persistedWatchdogSearch.SearchTerm.ShouldBe("text");
        _persistedWatchdogSearch.CurrentScrapingResults.ShouldBe(["<div>text</div>"]);
        _persistedWatchdogSearch.IsArchived.ShouldBe(false);
    }
}