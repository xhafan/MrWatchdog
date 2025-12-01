using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Statistics;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Statistics;

[TestFixture]
public class when_viewing_watchdog_detail_statistics_for_watchdog_without_any_search : BaseDatabaseTest
{
    private StatisticsModel _model = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new StatisticsModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_watchdog.Id);
    }

    [Test]
    public void number_of_watchdog_searches_is_correct()
    {
        _model.PublicWatchdogStatistics.NumberOfUsersWithWatchdogWithNotification.ShouldBe(0);
        _model.PublicWatchdogStatistics.NumberOfUsersWithWatchdogWithoutNotification.ShouldBe(0);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    } 
}