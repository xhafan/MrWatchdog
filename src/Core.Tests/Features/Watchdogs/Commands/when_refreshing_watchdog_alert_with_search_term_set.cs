using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_refreshing_watchdog_alert_with_search_term_set : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogAlert _watchdogAlert = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new RefreshWatchdogAlertCommandMessageHandler(
            new NhibernateRepository<WatchdogAlert>(UnitOfWork)
        );

        await handler.Handle(new RefreshWatchdogAlertCommand(_watchdogAlert.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        _watchdogAlert = UnitOfWork.LoadById<WatchdogAlert>(_watchdogAlert.Id);
    }

    [Test]
    public void watchdog_alert_is_refreshed()
    {
        _watchdogAlert.PreviousScrapingResults.ShouldBe(["Doom 1"]);
        _watchdogAlert.CurrentScrapingResults.ShouldBe(["Doom 1", "Doom 2"]);
        _watchdogAlert.ScrapingResultsToAlertAbout.ShouldBe(["Doom 2"]);
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1"]);

        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm("doom")
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Doom 2"]);
    }
}