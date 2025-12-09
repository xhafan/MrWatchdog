using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Badges;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Badges;

[TestFixture]
public class when_viewing_watchdog_detail_badges_for_archived_watchdog : BaseDatabaseTest
{
    private BadgesModel _model = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new BadgesModelBuilder(UnitOfWork).Build();
        
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
        _model.WatchdogDetailArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogDetailArgs.IsArchived.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
        _watchdog.Archive();
    }    
}