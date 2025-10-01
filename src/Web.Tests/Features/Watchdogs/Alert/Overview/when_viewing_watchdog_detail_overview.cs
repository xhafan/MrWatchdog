using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Alert.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alert.Overview;

[TestFixture]
public class when_viewing_watchdog_detail_overview : BaseDatabaseTest
{
    private OverviewModel _model = null!;
    private WatchdogAlert _watchdogAlert = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OverviewModelBuilder(UnitOfWork).Build();
        
        _actionResult = await _model.OnGet(_watchdogAlert.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogAlertOverviewArgs.WatchdogAlertId.ShouldBe(_watchdogAlert.Id);
        _model.WatchdogAlertOverviewArgs.SearchTerm.ShouldBe(WatchdogAlertBuilder.SearchTerm);
    }

    private void _BuildEntities()
    {
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork).Build();
    }    
}