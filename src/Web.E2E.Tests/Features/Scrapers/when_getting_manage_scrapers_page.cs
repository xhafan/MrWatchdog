using System.Net;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_getting_manage_scrapers_page : BaseDatabaseTest
{
    [Test]
    public async Task manage_scraper_page_redirects_to_login_page()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(ScraperUrlConstants.ScrapersManageUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith("/Account/Login?ReturnUrl=%2FScrapers%2FManage");
    }
}