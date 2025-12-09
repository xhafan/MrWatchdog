using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Search;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search;

[TestFixture]
public class when_archiving_watchdog_search : BaseDatabaseTest
{
    private SearchModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
    
    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _model = new SearchModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();
        
        _actionResult = await _model.OnPostArchiveWatchdogSearch(_watchdogSearch.Id);
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new ArchiveWatchdogSearchCommand(_watchdogSearch.Id))).MustHaveHappenedOnceExactly();
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
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork).Build();
    }
}