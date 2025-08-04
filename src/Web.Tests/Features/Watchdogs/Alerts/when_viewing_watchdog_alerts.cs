using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Alerts;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alerts;

[TestFixture]
public class when_viewing_watchdog_alerts : BaseDatabaseTest
{
    private WatchdogAlert _watchdogAlert = null!;
    private AlertsModel _model = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new AlertsModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogAlerts.ShouldContain(
            new GetWatchdogAlertsQueryResult
            {
                WatchdogAlertId = _watchdogAlert.Id,
                WatchdogName = "watchdog name",
                SearchTerm = "search term",
            }
        );
    }    
    
    private void _BuildEntities()
    {
        var watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .Build();

        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(watchdog)
            .WithSearchTerm("search term")
            .Build();
    }
}