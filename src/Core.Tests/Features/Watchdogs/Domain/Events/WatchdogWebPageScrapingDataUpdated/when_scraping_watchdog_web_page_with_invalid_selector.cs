using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using MrWatchdog.Core.Features.WatchDogs.Domain.Events.WatchdogWebPageScrapingFailed;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;
using System.Net;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;

[TestFixture]
public class when_scraping_watchdog_web_page_with_invalid_selector : BaseDatabaseTest
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
        webPage.ScrapingResults.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBe(null);
        webPage.ScrapingErrorMessage.ShouldBe("Unexpected token {Eoi} where one of [Ident, Char: *] was expected.");
    }
    
    [Test]
    public void watchdog_web_page_scraping_failed_domain_event_is_not_raised()
    {
        RaisedDomainEvents.ShouldNotContain(new WatchdogWebPageScrapingFailedDomainEvent(_watchdog.Id));
    }        
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = "div ",
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
    }
}