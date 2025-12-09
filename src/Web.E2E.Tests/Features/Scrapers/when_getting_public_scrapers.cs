using System.Net;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_getting_public_watchdogs : BaseDatabaseTest
{
    [Test]
    public async Task public_watchdogs_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(WatchdogUrlConstants.WachdogsPublicWatchdogsUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}