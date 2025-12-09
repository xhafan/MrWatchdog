using System.Net;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;

[TestFixture]
public class when_scraping_watchdog_web_page_with_multiple_html_nodes_selected : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

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
                        <a href="https://store.epicgames.com/en-US/p/two-point-hospital2" target="_blank">Two Point Hospital2</a>
                        </p>
                        </div>
                        </body>
                        </html>
                        """)
                }))
            .Build();
        
        var handler = new ScrapeWatchdogWebPageDomainEventMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            httpClientFactory
        );

        await handler.Handle(new WatchdogWebPageScrapingDataUpdatedDomainEvent(_watchdog.Id, _watchdogWebPageId));
    }

    [Test]
    public void web_page_is_scraped_and_scraping_results_are_set()
    {
        var webPage = _watchdog.WebPages.Single();
        webPage.ScrapingResults.ShouldBe([
            """
            <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
            """,
            """
            <a href="https://store.epicgames.com/en-US/p/two-point-hospital2" target="_blank">Two Point Hospital2</a>
            """
        ]);
        webPage.ScrapedOn.ShouldNotBeNull();
        webPage.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        webPage.ScrapingErrorMessage.ShouldBe(null);
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
    }
}