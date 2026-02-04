using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_updating_existing_scraper_web_page : BaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _scraper.UpdateWebPage(new ScraperWebPageArgs
        {
            ScraperWebPageId = _scraperWebPageId,
            Url = "http://url.com/page_updated",
            Selector = ".selector_updated",
            SelectText = true,
            Name = "url.com/page_updated",
            HttpHeaders = """
                          User-Agent: Mozilla/5.0
                          Connection: keep-alive
                          """
        });
    }

    [Test]
    public void scraper_web_page_is_updated()
    {
        var scraperWebPage = _scraper.WebPages.ShouldHaveSingleItem();
        scraperWebPage.Url.ShouldBe("http://url.com/page_updated");
        scraperWebPage.Selector.ShouldBe(".selector_updated");
        scraperWebPage.SelectText.ShouldBe(true);
        scraperWebPage.Name.ShouldBe("url.com/page_updated");

        scraperWebPage.HttpHeaders.Count().ShouldBe(2);
        scraperWebPage.HttpHeaders.ShouldContain(
            new ScraperWebPageHttpHeader("User-Agent", "Mozilla/5.0"));
        scraperWebPage.HttpHeaders.ShouldContain(new ScraperWebPageHttpHeader("Connection", "keep-alive"));
    }

    [Test]
    public void scraper_web_page_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new ScraperWebPageScrapingDataUpdatedDomainEvent(_scraper.Id, _scraperWebPageId));
    }

    [Test]
    public void web_page_scraped_results_and_scraped_on_are_reset()
    {
        var scraperWebPage = _scraper.WebPages.Single();
        scraperWebPage.ScrapedResults.ShouldBeEmpty();
        scraperWebPage.ScrapedOn.ShouldBe(null);
    }

    [Test]
    public void scraper_web_page_is_disabled()
    {
        var scraperWebPage = _scraper.WebPages.Single();
        scraperWebPage.IsEnabled.ShouldBe(false);
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
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapedResults(_scraperWebPageId, ["<div>x</div>"]);
        _scraper.EnableWebPage(_scraperWebPageId);
    }
}