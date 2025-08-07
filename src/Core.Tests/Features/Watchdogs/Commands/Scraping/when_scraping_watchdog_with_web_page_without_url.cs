using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.Scraping;

[TestFixture]
public class when_scraping_watchdog_with_web_page_without_url : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var httpClientFactory = new HttpClientFactoryBuilder().Build();
        
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
    public void watchdog_web_page_is_not_scraped()
    {
        var webPage = _watchdog.WebPages.Single();
        webPage.ScrapingResults.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBe(null);
        webPage.ScrapingErrorMessage.ShouldBe(null);
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = null,
                Selector = null,
                Name = null
            })
            .Build();
    }
}