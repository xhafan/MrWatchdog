using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_getting_watchdog_detail_page : BaseDatabaseTest
{
    private Watchdog? _watchdog;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task watchdog_detail_page_redirects_to_login_page()
    {
        var url = WatchdogUrlConstants.WatchdogDetailUrlTemplate.WithWatchdogId(_watchdog!.Id);
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith($"/Account/Login?ReturnUrl=%2FWatchdogs%2FDetail%2F{_watchdog!.Id}");
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
            }
        );
    }
}