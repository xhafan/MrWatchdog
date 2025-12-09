using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

[TestFixture]
public class when_creating_watchdog_search : BaseDatabaseTest
{
    private ScrapingResultsModel _model = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
    private User _actingUser = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _model = new ScrapingResultsModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithActingUser(_actingUser)
            .WithSearchTerm(" search term ")
            .Build();
        
        _actionResult = await _model.OnPostCreateWatchdogSearch(_watchdog.Id);
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new CreateWatchdogSearchCommand(_watchdog.Id, "search term"))).MustHaveHappenedOnceExactly();
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

        _actingUser = new UserBuilder(UnitOfWork).Build();
    }    
}