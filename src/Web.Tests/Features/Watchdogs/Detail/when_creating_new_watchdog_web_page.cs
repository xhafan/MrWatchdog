using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

[TestFixture]
public class when_creating_new_watchdog_web_page : BaseDatabaseTest
{
    private DetailModel _model = null!;
    private Watchdog _watchdog = null!;
    private IBus _bus = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _bus = A.Fake<IBus>();
        
        _model = new DetailModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();
        
        _actionResult = await _model.OnPostCreateWatchdogWebPage(_watchdog.Id);
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(
                A<CreateWatchdogWebPageCommand>.That.Matches(p => p.WatchdogId == _watchdog.Id
                                                                  && !p.Guid.Equals(Guid.Empty)),
                A<IDictionary<string, string>>._
            )
        ).MustHaveHappenedOnceExactly();
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
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}