using System.Net;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.Scraping;

[TestFixture]
public class when_scraping_watchdog_with_next_scraping_on_timestamp_already_set : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private readonly DateTime _nextScrapingOn = DateTime.UtcNow.AddSeconds(-5);

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
    public void watchdog_next_scraping_on_is_set()
    {
        _watchdog.NextScrapingOn.ShouldNotBeNull();
        _watchdog.NextScrapingOn.Value.ShouldBe(_nextScrapingOn.AddSeconds(60), tolerance: TimeSpan.FromMilliseconds(1));
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
            .WithScrapingIntervalInSeconds(60)
            .WithNextScrapingOn(_nextScrapingOn)
            .Build();
    }
}