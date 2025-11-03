using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Overview;

[TestFixture]
public class when_updating_watchdog_detail_overview : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private ICoreBus _bus = null!;    
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        _bus = A.Fake<ICoreBus>();
        
        _model = new OverviewModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();
        _model.WatchdogOverviewArgs = new WatchdogOverviewArgs
        {
            WatchdogId = _watchdog.Id,
            Name = "watchdog updated name",
            Description = "watchdog updated description",
            ScrapingIntervalInSeconds = 60,
            IntervalBetweenSameResultNotificationsInDays = 30,
            NumberOfFailedScrapingAttemptsBeforeAlerting = 5
        };

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new UpdateWatchdogOverviewCommand(_model.WatchdogOverviewArgs)))
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
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}