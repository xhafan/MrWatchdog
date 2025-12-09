using System.Net;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_getting_watchdogs_page : BaseDatabaseTest
{
    [Test]
    public async Task watchdogs_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(WatchdogUrlConstants.WatchdogsUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}