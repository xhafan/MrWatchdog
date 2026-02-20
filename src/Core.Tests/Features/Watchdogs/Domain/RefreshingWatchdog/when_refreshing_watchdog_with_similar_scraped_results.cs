using CoreUtils.Extensions;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdog;

[TestFixture]
public class when_refreshing_watchdog_with_similar_scraped_results : BaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _watchdog.Refresh();
    }

    [Test]
    public void current_scraped_results_are_correct()
    {
        _watchdog.CurrentScrapedResults.Count().ShouldBe(2);
        _watchdog.CurrentScrapedResults.First().Value.ShouldBe(
            """
            <a href="https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/">
            Vykoupení z věznice SHAWSHANK
            </a>
            """,
            ignoreLineEndings: true
        );
        _watchdog.CurrentScrapedResults.Second().Value.ShouldBe(
            """
            <a href="https://www.csfd.cz/film/12345-forrest-gump/prehled/" title="Forrest Gump">
            	Forrest Gump
            </a>
            """,
            ignoreLineEndings: true
        );
    }

    [Test]
    public void scraped_results_to_notify_about_are_correct()
    {
        _watchdog.ScrapedResultsToNotifyAbout.Count().ShouldBe(1);
        _watchdog.ScrapedResultsToNotifyAbout.First().Value.ShouldBe(
            """
            <a href="https://www.csfd.cz/film/12345-forrest-gump/prehled/" title="Forrest Gump">
            	Forrest Gump
            </a>
            """,
            ignoreLineEndings: true
        );
    }

    [Test]
    public void watchdog_scraped_results_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogScrapedResultsUpdatedDomainEvent(_watchdog.Id));
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapedResults(scraperWebPage.Id, [
            """
            <a href="https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/" title="Vykoupení z věznice Shawshank">
                Vykoupení z věznice Shawshank
            </a>
            """,
            """
            <a href="https://www.csfd.cz/film/10135-forrest-gump/prehled/" title="Forrest Gump">
            	Forrest Gump
            </a>
            """
        ]);
        _scraper.EnableWebPage(scraperWebPage.Id);

        _watchdog = new WatchdogBuilder()
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();

        _scraper.SetScrapedResults(scraperWebPage.Id, [
            """
            <a href="https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/">
            Vykoupení z věznice SHAWSHANK
            </a>
            """,
            """
            <a href="https://www.csfd.cz/film/12345-forrest-gump/prehled/" title="Forrest Gump">
            	Forrest Gump
            </a>
            """
        ]);
    }
}