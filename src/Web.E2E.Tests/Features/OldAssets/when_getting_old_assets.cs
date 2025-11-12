using MrWatchdog.TestsShared;
using System.Net;

namespace MrWatchdog.Web.E2E.Tests.Features.OldAssets;

[TestFixture]
public class when_getting_old_assets : BaseDatabaseTest
{
    [Test]
    public async Task js_bundle_with_invalid_fingerprint_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync("/assets/bundle.1234abcd.js");
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith("/assets/bundle.js");
    }

    [Test]
    public async Task css_bundle_with_invalid_fingerprint_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync("/assets/bundle.1234abcd.css");
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith("/assets/bundle.css");
    }
}