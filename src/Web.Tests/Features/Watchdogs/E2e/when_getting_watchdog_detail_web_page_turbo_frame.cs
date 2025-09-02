using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdog_detail_web_page_turbo_frame : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task watchdog_detail_web_page_turbo_frame_page_can_be_fetched()
    {
        var watchdogDetailUrl = WatchdogUrlConstants.WatchdogDetailWebPageTurboFrameUrl
            .Replace(WatchdogUrlConstants.WatchdogIdVariable, $"{_watchdog.Id}")
            .Replace(WatchdogUrlConstants.WatchdogWebPageIdVariable, $"{_watchdog.WebPages.Single().Id}");
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(watchdogDetailUrl);
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

        _watchdog = new WatchdogBuilder(newUnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
    }
}