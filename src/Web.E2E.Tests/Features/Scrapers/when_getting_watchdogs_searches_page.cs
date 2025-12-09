using System.Net;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_getting_watchdogs_searches_page : BaseDatabaseTest
{
    [Test]
    public async Task not_authenticated_watchdog_searches_page_redirects_to_login_page()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(WatchdogUrlConstants.WatchdogsSearchesUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith("/Account/Login?ReturnUrl=%2FWatchdogs%2FSearches");
    }
}