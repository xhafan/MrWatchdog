using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.CreatingWatchdogAlert;

[TestFixture]
public class when_creating_duplicate_watchdog_alert : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogAlert _watchdogAlert = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new CreateWatchdogAlertCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            new NhibernateRepository<WatchdogAlert>(UnitOfWork),
            new UserRepository(UnitOfWork),
            UnitOfWork
        );

        await handler.Handle(new CreateWatchdogAlertCommand(_watchdog.Id, SearchTerm: "text") { ActingUserId = _user.Id});
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
    }

    [Test]
    public void new_watchdog_alert_is_not_created()
    {
        UnitOfWork.Session!.Query<WatchdogAlert>().Single(x => x.Watchdog == _watchdog).ShouldBe(_watchdogAlert);
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>text 1</div>", "<div>string 2</div>", "<div>text 3</div>"]);
        
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm("text")
            .WithUser(_user)
            .Build();
    }
}