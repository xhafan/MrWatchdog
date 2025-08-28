using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdogs_alerts_page : BaseDatabaseTest
{
    [Test]
    public async Task watchdog_alerts_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(WatchdogWebConstants.WatchdogsAlertsUrl);
        response.EnsureSuccessStatusCode();
    }
}