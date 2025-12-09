using System.Net;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;

[TestFixture]
public class when_scraping_scraper_web_page_with_dangerous_html : BaseDatabaseTest
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
                        <p class="infoUpdate-log">text one<script>console.log("Hello from the script!");</script></p>
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
    public void dangerous_html_is_removed()
    {
        var webPage = _scraper.WebPages.Single();
        webPage.ScrapingResults.ShouldBe([
            """
            <p>text one</p>
            """
        ]);
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
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }
}