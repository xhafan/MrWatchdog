using System.Net;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_getting_user_watchdogs : BaseDatabaseTest
{
    [Test]
    public async Task user_watchdogs_redirect_to_login_page()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(WatchdogUrlConstants.WatchdogsUserWatchdogsUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith($"/Account/Login?ReturnUrl=%2FWatchdogs%2FUserWatchdogs");
    }
}