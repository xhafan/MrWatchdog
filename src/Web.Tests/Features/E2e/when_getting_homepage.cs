using System.Net;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests.Features.E2e;

[TestFixture]
public class when_getting_homepage : BaseDatabaseTest
{
    [Test]
    public async Task homepage_can_be_fetched_unauthenticated()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}