using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_creating_watchdog_alert : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogAlert? _watchdogAlert;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new CreateWatchdogAlertCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            new NhibernateRepository<WatchdogAlert>(UnitOfWork),
            UnitOfWork
        );

        await handler.Handle(new CreateWatchdogAlertCommand(_watchdog.Id, SearchTerm: "text"));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogAlert = UnitOfWork.Session!.Query<WatchdogAlert>()
            .SingleOrDefault(x => x.Watchdog == _watchdog);
    }

    [Test]
    public void new_watchdog_alert_is_created_with_matching_current_scraping_results()
    {
        _watchdogAlert.ShouldNotBeNull();
        _watchdogAlert.SearchTerm.ShouldBe("text");
        _watchdogAlert.PreviousScrapingResults.ShouldBeEmpty();
        _watchdogAlert.CurrentScrapingResults.ShouldBe(["<div>tÉxt 1</div>", "<div>texŤ 3</div>"]);
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
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>tÉxt 1</div>", "<div>string 2</div>", "<div>texŤ 3</div>"]);
    }
}