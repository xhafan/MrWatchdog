using System.Net;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features;

[TestFixture]
public class when_getting_error_page : BaseDatabaseTest
{
    [Test]
    public async Task error_page_can_be_fetched_unauthenticated()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync("/Error");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}