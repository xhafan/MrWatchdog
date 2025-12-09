using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

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
                _watchdog = new WatchdogBuilder(newUnitOfWork).Build();
                _watchdog.MakePublic();
            }
        );
    }
}