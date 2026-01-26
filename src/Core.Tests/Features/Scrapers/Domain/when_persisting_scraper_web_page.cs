using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_persisting_scraper_web_page : BaseDatabaseTest
{
    private Scraper _newScraper = null!;
    private ScraperWebPage _newScraperWebPage = null!;
    private ScraperWebPage _persistedScraperWebPage = null!;

    [SetUp]
    public void Context()
    {
        _newScraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                ScrapeHtmlAsRenderedByBrowser = true,
                ScrapingByBrowserWaitFor = ScrapingByBrowserWaitFor.NetworkIdle,
                SelectText = true,
                Name = "url.com/page"
            })
            .Build();
        _newScraperWebPage = _newScraper.WebPages.Single();
        _newScraper.SetScrapingResults(_newScraperWebPage.Id, ["<div>text</div>"]);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedScraperWebPage = UnitOfWork.LoadById<Scraper>(_newScraper.Id).WebPages.Single();
    }

    [Test]
    public void persisted_scraper_web_page_can_be_retrieved_and_has_correct_data()
    {
        _persistedScraperWebPage.ShouldBe(_newScraperWebPage);
        _persistedScraperWebPage.Scraper.ShouldBe(_newScraper);
        _persistedScraperWebPage.Url.ShouldBe("http://url.com/page");
        _persistedScraperWebPage.Selector.ShouldBe(".selector");
        _persistedScraperWebPage.ScrapeHtmlAsRenderedByBrowser.ShouldBe(true);
        _persistedScraperWebPage.ScrapingByBrowserWaitFor.ShouldBe(ScrapingByBrowserWaitFor.NetworkIdle);
        _persistedScraperWebPage.SelectText.ShouldBe(true);
        _persistedScraperWebPage.Name.ShouldBe("url.com/page");
        _persistedScraperWebPage.ScrapingResults.ShouldBe(["text"]);
        _persistedScraperWebPage.ScrapedOn.ShouldNotBeNull();
        _persistedScraperWebPage.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _persistedScraperWebPage.IsEnabled.ShouldBe(false);
        _persistedScraperWebPage.NumberOfFailedScrapingAttemptsBeforeTheNextAlert.ShouldBe(0);
    }
}