using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_getting_watchdog_detail_web_page_scraping_results : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task watchdog_detail_web_page_overview_redirects_to_login_page()
    {
        var watchdogWebPageId = _watchdog.WebPages.Single().Id;

        var url = WatchdogUrlConstants.WatchdogDetailWebPageScrapingResultsUrlTemplate
            .WithWatchdogId(_watchdog.Id)
            .WithWatchdogWebPageIdVariable(watchdogWebPageId);
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith(
            $"/Account/Login?ReturnUrl=%2FWatchdogs%2FDetail%2FWebPage%2FWebPageScrapingResults%3FwatchdogId%3D{_watchdog.Id}%26watchdogWebPageId%3D{watchdogWebPageId}");
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteWatchdogCascade(_watchdog);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _watchdog = new WatchdogBuilder(newUnitOfWork)
                    .WithWebPage(new WatchdogWebPageArgs
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