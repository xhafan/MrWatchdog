using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Overview;

[TestFixture]
public class when_viewing_watchdog_detail_overview : BaseDatabaseTest
{
    private OverviewModel _model = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OverviewModelBuilder(UnitOfWork)
            .Build();
        
        _actionResult = await _model.OnGet(_watchdog.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogOverviewArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogOverviewArgs.Name.ShouldBe(WatchdogBuilder.Name);
        _model.WatchdogOverviewArgs.Description.ShouldBe(WatchdogBuilder.Description);
        _model.WatchdogOverviewArgs.ScrapingIntervalInSeconds.ShouldBe(WatchdogBuilder.ScrapingIntervalInSeconds);
        _model.WatchdogOverviewArgs.IntervalBetweenSameResultNotificationsInDays.ShouldBe(WatchdogBuilder.IntervalBetweenSameResultNotificationsInDays);
        _model.WatchdogOverviewArgs.NumberOfFailedScrapingAttemptsBeforeAlerting.ShouldBe(WatchdogBuilder.NumberOfFailedScrapingAttemptsBeforeAlerting);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}