using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_getting_scraper_detail_web_page_overview : BaseDatabaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task scraper_detail_web_page_overview_redirects_to_login_page()
    {
        var scraperWebPageId = _scraper.WebPages.Single().Id;

        var url = ScraperUrlConstants.ScraperDetailWebPageOverviewUrlTemplate
            .WithScraperId(_scraper.Id)
            .WithScraperWebPageIdVariable(scraperWebPageId);
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith(
            $"/Account/Login?ReturnUrl=%2FScrapers%2FDetail%2FWebPage%2FWebPageOverview%3FscraperId%3D{_scraper.Id}%26scraperWebPageId%3D{scraperWebPageId}");
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteScraperCascade(_scraper);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _scraper = new ScraperBuilder(newUnitOfWork)
                    .WithWebPage(new ScraperWebPageArgs
                    {
                        Url = "http://url.com/page",
                        Selector = ".selector",
                        Name = "url.com/page"
                    })
                    .Build();
            }
        );
    }
}