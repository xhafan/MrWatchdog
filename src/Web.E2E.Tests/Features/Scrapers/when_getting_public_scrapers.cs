using System.Net;
using CoreBackend.TestsShared;
using MrWatchdog.Core.Features.Scrapers;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_getting_public_scrapers : BaseDatabaseTest
{
    [Test]
    public async Task public_scrapers_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(ScraperUrlConstants.ScrapersPublicScrapersUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}