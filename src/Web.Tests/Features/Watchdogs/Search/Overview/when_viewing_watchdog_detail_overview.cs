using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Search.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search.Overview;

[TestFixture]
public class when_viewing_watchdog_detail_overview : BaseDatabaseTest
{
    private OverviewModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OverviewModelBuilder(UnitOfWork).Build();
        
        _actionResult = await _model.OnGet(_watchdogSearch.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogSearchOverviewArgs.WatchdogSearchId.ShouldBe(_watchdogSearch.Id);
        _model.WatchdogSearchOverviewArgs.ReceiveNotification.ShouldBe(true);
        _model.WatchdogSearchOverviewArgs.SearchTerm.ShouldBe(WatchdogSearchBuilder.SearchTerm);
    }

    private void _BuildEntities()
    {
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithReceiveNotification(true)
            .Build();
    }    
}