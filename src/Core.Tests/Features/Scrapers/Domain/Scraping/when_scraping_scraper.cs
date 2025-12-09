using System.Net;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Scraping;

[TestFixture]
public class when_scraping_scraper : BaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://www.pcgamer.com/epic-games-store-free-games-list/",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                        <html>
                        <body>
                        <div id="article-body">
                        <p class="infoUpdate-log">
                        <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
                        </p>
                        </div>
                        </body>
                        </html>
                        """)
                }))
            .Build();

        await _scraper.Scrape(httpClientFactory);
    }

    [Test]
    public void scraper_web_page_is_scraped_and_scraping_results_are_set()
    {
        var webPage = _scraper.WebPages.Single();
        webPage.ScrapingResults.ShouldBe([
            """
            <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
            """
        ]);
        webPage.ScrapedOn.ShouldNotBeNull();
        webPage.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        webPage.ScrapingErrorMessage.ShouldBe(null);
    }

    [Test]
    public void scraper_scraping_completed_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new ScraperScrapingCompletedDomainEvent(_scraper.Id));
    }

    [Test]
    public void after_successful_scraping_of_all_web_pages_the_scraper_can_notify_about_failed_scraping()
    {
        _scraper.CanNotifyAboutFailedScraping.ShouldBe(true);
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
    }
}