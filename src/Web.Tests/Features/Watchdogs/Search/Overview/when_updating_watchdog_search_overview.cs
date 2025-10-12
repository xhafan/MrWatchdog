using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Search.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search.Overview;

[TestFixture]
public class when_updating_watchdog_search_overview : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private ICoreBus _bus = null!;
    private WatchdogSearch _watchdogSearch = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        _bus = A.Fake<ICoreBus>();

        _model = new OverviewModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithWatchdogSearchOverviewArgs(
                new WatchdogSearchOverviewArgs
                {
                    WatchdogSearchId = _watchdogSearch.Id,
                    SearchTerm = "new search term",
                }
            )
            .Build();

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new UpdateWatchdogSearchOverviewCommand(_model.WatchdogSearchOverviewArgs))).MustHaveHappenedOnceExactly();
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
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithSearchTerm("search term")
            .Build();
    }
    
}