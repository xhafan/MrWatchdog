using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.TestsShared.HttpClients;
using System.Net;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.Scraping;

[TestFixture]
public class when_scraping_watchdog : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

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
        
        var handler = new ScrapeWatchdogCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            httpClientFactory
        );

        await handler.Handle(new ScrapeWatchdogCommand(_watchdog.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);
    }

    [Test]
    public void watchdog_web_page_is_scraped_and_scraping_results_are_set()
    {
        var webPage = _watchdog.WebPages.Single();
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
    public void watchdog_scraping_completed_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogScrapingCompletedDomainEvent(_watchdog.Id));
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
    }
}