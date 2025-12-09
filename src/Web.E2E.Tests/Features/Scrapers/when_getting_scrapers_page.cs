using System.Net;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_getting_scrapers_page : BaseDatabaseTest
{
    [Test]
    public async Task scrapers_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(ScraperUrlConstants.ScrapersUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}