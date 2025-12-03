using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

[TestFixture]
public class when_viewing_watchdog_detail : BaseDatabaseTest
{
    private DetailModel _model = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new DetailModelBuilder(UnitOfWork).Build();
        
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
        _model.WatchdogDetailArgs.WebPageIds.ShouldBeEmpty();
        _model.WatchdogDetailArgs.Name.ShouldBe("watchdog name");
        _model.WatchdogDetailArgs.PublicStatus.ShouldBe(PublicStatus.RequestedToBeMadePublic);
        _model.WatchdogDetailArgs.UserId.ShouldBe(_watchdog.User.Id);
        _model.WatchdogDetailArgs.UserEmail.ShouldBe(_watchdog.User.Email);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .Build();
        _watchdog.RequestToMakePublic();
    }    
}