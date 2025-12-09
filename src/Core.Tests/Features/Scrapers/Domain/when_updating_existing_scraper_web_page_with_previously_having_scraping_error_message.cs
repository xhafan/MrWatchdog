using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_updating_existing_scraper_web_page_with_previously_having_scraping_error_message : BaseTest
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
            Name = "url.com/page_updated"
        });
    }

    [Test]
    public void web_page_scraping_error_message_is_reset()
    {
        var scraperWebPage = _scraper.WebPages.Single();
        scraperWebPage.ScrapingErrorMessage.ShouldBe(null);
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
        _scraper.SetScrapingErrorMessage(_scraperWebPageId, "Network error");
    }
}