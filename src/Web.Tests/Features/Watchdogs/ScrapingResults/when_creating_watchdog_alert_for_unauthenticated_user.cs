using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

[TestFixture]
public class when_creating_watchdog_alert_for_unauthenticated_user : BaseDatabaseTest
{
    private ScrapingResultsModel _model = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _model = new ScrapingResultsModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();
        
        _actionResult = await _model.OnPostCreateWatchdogAlert(_watchdog.Id, " search term ");
    }

    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new CreateWatchdogAlertCommand(_watchdog.Id, "search term"))).MustNotHaveHappened();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<UnauthorizedResult>();
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}