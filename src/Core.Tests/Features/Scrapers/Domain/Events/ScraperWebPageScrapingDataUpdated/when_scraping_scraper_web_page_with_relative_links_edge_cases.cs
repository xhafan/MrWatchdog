using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;
using System.Net;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;

[TestFixture]
public class when_scraping_scraper_web_page_with_relative_links_edge_cases : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://example.com/test/",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                        <html>
                        <body>
                        <div class="content">
                            <a href="/absolute/path">Absolute path</a>
                            <a href="relative/path">Relative path</a>
                            <img src="/absolute_images/test.jpg" />
                            <img src="relative_images/test.jpg" />
                            <a href="https://other.com/external">External link</a>
                            <a href="//protocol-relative.com/path">Protocol-relative</a>
                            <a href="#anchor">Anchor link</a>
                        </div>
                        </body>
                        </html>
                        """)
                }))
            .Build();
        
        var handler = new ScrapeScraperWebPageDomainEventMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            new WebScraperChain([new HttpClientScraper(httpClientFactory)])
        );

        await handler.Handle(new ScraperWebPageScrapingDataUpdatedDomainEvent(_scraper.Id, _scraperWebPageId));
    }

[Test]
    public void web_page_is_scraped_and_only_absolute_path_links_are_converted()
    {
        var webPage = _scraper.WebPages.Single();
        webPage.ScrapedResults.ShouldNotBeEmpty();
        var scrapedResult = webPage.ScrapedResults.First();
        
        scrapedResult.ShouldContain("""
                                     href="https://example.com/absolute/path"
                                     """);
        scrapedResult.ShouldContain("""
                                     href="relative/path"
                                     """);
        scrapedResult.ShouldContain("""
                                     src="https://example.com/absolute_images/test.jpg"
                                     """);
        scrapedResult.ShouldContain("""
                                     src="relative_images/test.jpg"
                                     """);
        scrapedResult.ShouldContain("""
                                     href="https://other.com/external"
                                     """);
        scrapedResult.ShouldContain("""
                                     href="//protocol-relative.com/path"
                                     """);
        scrapedResult.ShouldContain("""
                                     href="#anchor"
                                     """);
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://example.com/test/",
                Selector = "div.content",
                Name = "example.com/test/"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }
}