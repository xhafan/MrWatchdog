using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_updating_existing_watchdog_web_page : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new UpdateWatchdogWebPageCommandMessageHandler(new NhibernateRepository<Watchdog>(UnitOfWork));

        await handler.Handle(new UpdateWatchdogWebPageCommand(new WatchdogWebPageArgs
        {
            WatchdogId = _watchdog.Id,
            WatchdogWebPageId = _watchdogWebPageId,
            Url = "http://url.com/page_updated",
            Selector = ".selector_updated",
            Name = "url.com/page_updated"
        }));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);
    }

    [Test]
    public void watchdog_web_page_is_updated()
    {
        var watchdogWebPage = _watchdog.WebPages.ShouldHaveSingleItem();
        watchdogWebPage.Url.ShouldBe("http://url.com/page_updated");
        watchdogWebPage.Selector.ShouldBe(".selector_updated");
        watchdogWebPage.Name.ShouldBe("url.com/page_updated");
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
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
    }
}