using CoreDdd.Nhibernate.TestHelpers;
using FakeItEasy;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_refreshing_watchdog_scraped_results_hashes : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        await _BuildEntities();
        
        _watchdog.RefreshScrapedResultsHashes();
    }

    [Test]
    public void scraped_results_hashes_are_recalculated()
    {
        _watchdog.CurrentScrapedResults.Single().Hash.Length.ShouldBe(32);
        _watchdog.ScrapedResultsToNotifyAbout.Single().Hash.Length.ShouldBe(32);
        _watchdog.ScrapedResultsHistory.Single().ScrapedResult.Hash.Length.ShouldBe(32);
    }

    private async Task _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
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
            """
        ]);
        _scraper.EnableWebPage(scraperWebPage.Id);

        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();

        _scraper.SetScrapedResults(scraperWebPage.Id, [
            """
            <a href="https://www.csfd.cz/film/10135-forrest-gump/prehled/" title="Forrest Gump">
            	Forrest Gump
            </a>
            """
        ]);

        _watchdog.Refresh();
        await _watchdog.NotifyUserAboutNewScrapedResults(
            A.Fake<ICoreBus>(), 
            OptionsTestRetriever.Retrieve<RuntimeOptions>().Value,
            OptionsTestRetriever.Retrieve<JwtOptions>().Value
        );

        _scraper.SetScrapedResults(scraperWebPage.Id, [
            """
            <a href="https://www.csfd.cz/film/2292-zelena-mile/prehled/" title="Zelená míle" target="_blank">
            	Zelená míle
            </a>
            """
        ]);

        _watchdog.Refresh();
        await UnitOfWork.FlushAsync();

        UnitOfWork.Session!.Clear();

        _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);

        _simulateInvalidScrapedResultsHash();

        void _simulateInvalidScrapedResultsHash()
        {
            _watchdog.CurrentScrapedResults.Single().SetPrivateProperty(x => x.Hash, Array.Empty<byte>());
            _watchdog.ScrapedResultsToNotifyAbout.Single().SetPrivateProperty(x => x.Hash, Array.Empty<byte>());
            _watchdog.ScrapedResultsHistory.Single().ScrapedResult.SetPrivateProperty(x => x.Hash, Array.Empty<byte>());
        }
    }
}