using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Alerts;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alerts;

[TestFixture]
public class when_viewing_watchdog_alerts : BaseDatabaseTest
{
    private WatchdogAlert _watchdogAlertForUserOne = null!;
    private AlertsModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private WatchdogAlert _watchdogAlertForUserTwo = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new AlertsModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogAlerts.ShouldContain(
            new GetWatchdogAlertsQueryResult
            {
                WatchdogAlertId = _watchdogAlertForUserOne.Id,
                WatchdogName = "watchdog name",
                SearchTerm = "search term",
            }
        );
        _model.WatchdogAlerts.ShouldNotContain(
            new GetWatchdogAlertsQueryResult
            {
                WatchdogAlertId = _watchdogAlertForUserTwo.Id,
                WatchdogName = "watchdog name",
                SearchTerm = "search term",
            }
        );        
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();
        
        var watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .Build();

        _watchdogAlertForUserOne = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(watchdog)
            .WithSearchTerm("search term")
            .WithUser(_userOne)
            .Build();

        _watchdogAlertForUserTwo = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(watchdog)
            .WithSearchTerm("search term")
            .WithUser(_userTwo)
            .Build();
    }
}