using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Scraping;

[TestFixture]
public class when_scraping_scraper_with_web_page_without_url : BaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var httpClientFactory = new HttpClientFactoryBuilder().Build();
        
        await _scraper.Scrape(new WebScraperChain([new HttpClientScraper(httpClientFactory)]));
    }

    [Test]
    public void scraper_web_page_is_not_scraped()
    {
        var webPage = _scraper.WebPages.Single();
        webPage.ScrapedResults.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBe(null);
        webPage.ScrapingErrorMessage.ShouldBe(null);
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = null,
                Selector = null,
                Name = null
            })
            .Build();
    }
}