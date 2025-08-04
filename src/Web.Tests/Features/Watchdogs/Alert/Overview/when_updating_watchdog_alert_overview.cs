using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Alert.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alert.Overview;

[TestFixture]
public class when_updating_watchdog_alert_overview : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private ICoreBus _bus = null!;
    private WatchdogAlert _watchdogAlert = null!;
    private WatchdogAlertOverviewArgs _watchdogAlertOverviewArgs = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        _bus = A.Fake<ICoreBus>();

        _model = new OverviewModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithWatchdogAlertOverviewArgs(
                new WatchdogAlertOverviewArgs
                {
                    WatchdogAlertId = _watchdogAlert.Id,
                    SearchTerm = "new search term",
                }
            )
            .Build();
        _model.WatchdogAlertOverviewArgs = _watchdogAlertOverviewArgs;

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new UpdateWatchdogAlertOverviewCommand(_watchdogAlertOverviewArgs))).MustHaveHappenedOnceExactly();
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

    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    private void _BuildEntities()
    {
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithSearchTerm("search term")
            .Build();
    }
    
}