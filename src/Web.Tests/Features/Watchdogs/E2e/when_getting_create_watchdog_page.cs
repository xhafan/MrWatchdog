using System.Net;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_create_watchdog_page : BaseDatabaseTest
{
    [Test]
    public async Task create_watchdog_page_redirects_to_login_page()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(WatchdogUrlConstants.WatchdogCreateUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith($"/Account/Login?ReturnUrl=%2FWatchdogs%2FCreate");
    }
}