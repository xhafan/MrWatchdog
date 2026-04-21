using System.Net;
using CoreBackend.TestsShared;
using MrWatchdog.Core.Features.Account;

namespace MrWatchdog.Web.E2E.Tests.Features.Account.LoginLink;

[TestFixture]
public class when_getting_login_page : BaseDatabaseTest
{
    [Test]
    public async Task login_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(AccountUrlConstants.AccountLoginUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}