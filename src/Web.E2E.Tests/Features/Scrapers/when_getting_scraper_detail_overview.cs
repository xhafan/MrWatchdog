using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_getting_scraper_detail_overview : BaseDatabaseTest
{
    private Scraper? _scraper;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task scraper_detail_overview_page_redirects_to_login_page()
    {
        var url = ScraperUrlConstants.ScraperDetailOverviewUrlTemplate.WithScraperId(_scraper!.Id);
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith($"/Account/Login?ReturnUrl=%2FScrapers%2FDetail%2FOverview%2F{_scraper!.Id}");
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
                _scraper = new ScraperBuilder(newUnitOfWork).Build();
            }
        );
    }
}