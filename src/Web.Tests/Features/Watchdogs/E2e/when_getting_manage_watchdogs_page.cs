using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_manage_watchdogs_page : BaseDatabaseTest
{
    [Test]
    public async Task manage_watchdog_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(WatchdogWebConstants.ManageWatchdogsUrl);
        response.EnsureSuccessStatusCode();
    }
}