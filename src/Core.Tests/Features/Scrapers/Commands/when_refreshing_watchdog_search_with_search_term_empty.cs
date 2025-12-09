using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_refreshing_watchdog_search_with_search_term_empty : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogSearch _watchdogSearch = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new RefreshWatchdogSearchCommandMessageHandler(
            new NhibernateRepository<WatchdogSearch>(UnitOfWork)
        );

        await handler.Handle(new RefreshWatchdogSearchCommand(_watchdogSearch.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        _watchdogSearch = UnitOfWork.LoadById<WatchdogSearch>(_watchdogSearch.Id);
    }

    [Test]
    public void watchdog_search_is_refreshed()
    {
        _watchdogSearch.CurrentScrapingResults.ShouldBe(["Doom 1", "Prince Of Persia"]);
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe(["Prince Of Persia"]);
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
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Another World", "Doom 1"]);
        _watchdog.EnableWebPage(watchdogWebPage.Id);

        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Prince Of Persia"]);
    }
}