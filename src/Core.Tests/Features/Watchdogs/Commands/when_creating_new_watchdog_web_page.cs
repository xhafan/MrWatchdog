using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_creating_new_watchdog_web_page : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new CreateWatchdogWebPageCommandMessageHandler(new NhibernateRepository<Watchdog>(UnitOfWork));

        await handler.Handle(new CreateWatchdogWebPageCommand(_watchdog.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);
    }

    [Test]
    public void empty_watchdog_web_page_is_created()
    {
        var watchdogWebPage = _watchdog.WebPages.ShouldHaveSingleItem();
        watchdogWebPage.Url.ShouldBe(null);
        watchdogWebPage.Selector.ShouldBe(null);
        watchdogWebPage.Name.ShouldBe(null);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }
}