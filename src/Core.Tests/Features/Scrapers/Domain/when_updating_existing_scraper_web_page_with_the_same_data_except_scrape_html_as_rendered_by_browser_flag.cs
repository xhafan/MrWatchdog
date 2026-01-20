using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_updating_existing_scraper_web_page_with_the_same_data_except_scrape_html_as_rendered_by_browser_flag : BaseTest
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
            Url = "http://url.com/page",
            Selector = ".selector",
            ScrapeHtmlAsRenderedByBrowser = true,
            SelectText = false,
            Name = "url.com/page"
        });
    }

    [Test]
    public void scraper_web_page_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new ScraperWebPageScrapingDataUpdatedDomainEvent(_scraper.Id, _scraperWebPageId));
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                ScrapeHtmlAsRenderedByBrowser = false,
                SelectText = false,
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }
}