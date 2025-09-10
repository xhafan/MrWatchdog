using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;
using System.Net;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_manage_watchdogs_page : BaseDatabaseTest
{
    [Test]
    public async Task manage_watchdog_page_redirects_to_login_page()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(WatchdogUrlConstants.ManageWatchdogsUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith("/Account/Login?ReturnUrl=%2FWatchdogs%2FManage");
    }
}