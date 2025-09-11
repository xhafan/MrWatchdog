using System.Net;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests.Features.Account.E2e;

[TestFixture]
public class when_getting_login_page : BaseDatabaseTest
{
    [Test]
    public async Task login_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(AccountUrlConstants.AccountLoginUrlTemplate);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}