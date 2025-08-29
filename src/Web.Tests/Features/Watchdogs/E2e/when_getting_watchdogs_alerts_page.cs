using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;
using System.Net;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdogs_alerts_page : BaseDatabaseTest
{
    [Test]
    public async Task not_authenticated_watchdog_alerts_page_redirects_to_login_page()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(WatchdogUrlConstants.WatchdogsAlertsUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith("/Account/Login?ReturnUrl=%2FWatchdogs%2FAlerts");
    }
}