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
        var watchdogWebPageId = _watchdog.WebPages.Single().Id;

        var url = WatchdogUrlConstants.WatchdogDetailWebPageTurboFrameUrlTemplate
            .WithWatchdogId(_watchdog.Id)
            .WithWatchdogWebPageIdVariable(watchdogWebPageId);
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith(
            $"/Account/Login?ReturnUrl=%2FWatchdogs%2FDetail%2FWebPage%2FWebPageTurboFrame%3FwatchdogId%3D{_watchdog.Id}%26watchdogWebPageId%3D{watchdogWebPageId}");
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