using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Scraping;

[TestFixture]
public class when_scraping_scraper_with_non_existent_server : BaseTest
{
    private Scraper _scraper = null!;
    private WebScraperChain _webScraperChain = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _webScraperChain = new WebScraperChain([
            new HttpClientScraper(new HttpClientFactory()),
            new PlaywrightScraper(
                await RunOncePerTestRun.PlaywrightTask.Value,
                OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>()
            )
        ]);

        await _scraper.Scrape(_webScraperChain);
    }

    [Test]
    public void only_first_web_scraper_is_executed()
    {
        var webPage = _scraper.WebPages.Single();

        webPage.ScrapingErrorMessage.ShouldNotBeNull();
        webPage.ScrapingErrorMessage.ShouldContain("Scraping failed:");
        webPage.ScrapingErrorMessage.ShouldContain("HttpClientScraper:");
        webPage.ScrapingErrorMessage.ShouldNotContain("PlaywrightScraper:");

        webPage.ScrapedResults.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBeNull();
    }
    
   private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = $"https://non_existent_domain{Guid.NewGuid()}.com",
                Selector = "html",
                Name = "non_existent_domain"
            })
            .Build();
    }
}