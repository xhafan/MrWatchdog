using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdog_scraping_results : BaseDatabaseTest
{
    private Watchdog? _watchdog;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task watchdog_scraping_results_page_can_be_fetched_unauthenticated()
    {
        var url = WatchdogUrlConstants.WatchdogScrapingResultsUrlTemplate.WithWatchdogId(_watchdog!.Id);
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        await newUnitOfWork.DeleteWatchdogCascade(_watchdog);
    } 

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _watchdog = new WatchdogBuilder(newUnitOfWork).Build();
    }
}