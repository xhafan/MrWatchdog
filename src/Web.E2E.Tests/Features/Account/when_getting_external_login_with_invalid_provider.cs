using System.Net;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Account;

[TestFixture]
public class when_getting_external_login_with_invalid_provider : BaseDatabaseTest
{
    [Test]
    public async Task external_login_fails()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(
            AccountUrlConstants.AccountExternalLoginUrl.WithProvider("GoogleGoogleGoogleGoogleGoogleGoogleGoogleGoogleGoogleGoogleGoogle")
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}