using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdog_alert_page : BaseDatabaseTest
{
    private WatchdogAlert? _watchdogAlert;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task watchdog_alert_page_can_be_fetched()
    {
        var watchdogAlertUrl = WatchdogUrlConstants.WatchdogAlertUrl
            .Replace(WatchdogUrlConstants.WatchdogAlertIdVariable, $"{_watchdogAlert!.Id}");
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(watchdogAlertUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        await newUnitOfWork.DeleteWatchdogAlertCascade(_watchdogAlert);
    } 

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _watchdogAlert = new WatchdogAlertBuilder(newUnitOfWork).Build();
    }
}