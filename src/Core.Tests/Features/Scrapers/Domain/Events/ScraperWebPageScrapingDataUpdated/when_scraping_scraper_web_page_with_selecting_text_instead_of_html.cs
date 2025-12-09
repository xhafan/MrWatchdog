using System.Net;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;

[TestFixture]
public class when_scraping_scraper_web_page_with_selecting_text_instead_of_html : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

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
                        One<a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">  Two Point &amp; Hospital  </a>Two
                        </p>
                        </div>
                        </body>
                        </html>
                        """)
                }))
            .Build();
        
        var handler = new ScrapeScraperWebPageDomainEventMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            httpClientFactory
        );

        await handler.Handle(new ScraperWebPageScrapingDataUpdatedDomainEvent(_scraper.Id, _scraperWebPageId));
    }

    [Test]
    public void web_page_is_scraped_and_scraping_results_values_are_text_instead_of_html()
    {
        var webPage = _scraper.WebPages.Single();
        webPage.ScrapingResults.ShouldBe(["One Two Point & Hospital Two"]);
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log
                           """,
                SelectText = true,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingErrorMessage(_scraperWebPageId, "Network error");
    }
}