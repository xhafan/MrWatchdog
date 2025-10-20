using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Statistics;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Statistics;

[TestFixture]
public class when_viewing_watchdog_detail_statistics : BaseDatabaseTest
{
    private StatisticsModel _model = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new StatisticsModelBuilder(UnitOfWork)
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
        _model.WatchdogId.ShouldBe(_watchdog.Id);

        _model.PublicWatchdogStatistics.CalculatedEarningsForThisMonth.ShouldBe(0);
        _model.PublicWatchdogStatistics.NumberOfUsersWithWatchdogSearchWithNotification.ShouldBe(2);
        _model.PublicWatchdogStatistics.NumberOfUsersWithWatchdogSearchWithoutNotification.ShouldBe(1);

        _model.WatchdogPublicStatus.ShouldBe(PublicStatus.Public);
    }

    private void _BuildEntities()
    {
        var watchdogOwner = new UserBuilder(UnitOfWork).Build();

        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithUser(watchdogOwner)
            .Build();
        _watchdog.MakePublic();

        // ReSharper disable UnusedVariable
        var ownersWatchdogSearchWithNotification = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(watchdogOwner)
            .WithReceiveNotification(true)
            .Build();

        var ownersWatchdogSearchWithoutNotification = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(watchdogOwner)
            .WithReceiveNotification(false)
            .Build();

        var userOne = new UserBuilder(UnitOfWork).Build();
        var userTwo = new UserBuilder(UnitOfWork).Build();

        var watchdogSearchOneWithNotification = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(userOne)
            .WithReceiveNotification(true)
            .Build();

        var watchdogSearchTwoWithNotification = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(userOne)
            .WithReceiveNotification(true)
            .Build();

        var watchdogSearchThreeWithoutNotification = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(userTwo)
            .WithReceiveNotification(true)
            .Build();

        var watchdogSearchFourWithoutNotification = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(userTwo)
            .WithReceiveNotification(false)
            .Build();

        var watchdogSearchFiveWithoutNotification = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(userTwo)
            .WithReceiveNotification(false)
            .Build();

        var watchdogSearchForAnotherWatchdog = new WatchdogSearchBuilder(UnitOfWork)
            .Build();
        // ReSharper restore UnusedVariable
    }    
}