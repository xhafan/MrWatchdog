using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_removing_watchdog_web_page : BaseDatabaseTest
{
    private WebPageModel _model = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _bus = A.Fake<ICoreBus>();
        
        _model = new WebPageModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();
        
        _actionResult = await _model.OnPostRemoveWatchdogWebPage(_watchdog.Id, _watchdogWebPageId);
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new RemoveWatchdogWebPageCommand(_watchdog.Id, _watchdogWebPageId)))
            .MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<OkObjectResult>();
        var okObjectResult = (OkObjectResult) _actionResult;
        var value = okObjectResult.Value;
        value.ShouldBeOfType<string>();
        var jobGuid = (string) value;
        jobGuid.ShouldMatch(@"[0-9A-Fa-f\-]{36}");
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