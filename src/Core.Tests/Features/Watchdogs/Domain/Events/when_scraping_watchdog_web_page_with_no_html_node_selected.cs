using System.Net;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events;

[TestFixture]
public class when_scraping_watchdog_web_page_with_no_html_node_selected : BaseDatabaseTest
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
                        <a href="https://invalid_url" target="_blank">Two Point Hospital</a>
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
    public void web_page_scraping_error_message_is_set()
    {
        var webPage = _watchdog.WebPages.Single();
        webPage.SelectedElements.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBe(null);
        webPage.ScrapingErrorMessage.ShouldBe("No HTML node selected. Please review the Selector.");
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