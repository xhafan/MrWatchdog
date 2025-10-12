using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdog_search_overview : BaseDatabaseTest
{
    private WatchdogSearch? _watchdogSearch;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task watchdog_search_overview_page_redirects_to_login_page()
    {
        var url = WatchdogUrlConstants.WatchdogSearchOverviewUrlTemplate.WithWatchdogSearchId(_watchdogSearch!.Id);
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith($"/Account/Login?ReturnUrl=%2FWatchdogs%2FSearch%2FOverview%2F{_watchdogSearch.Id}");
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        await newUnitOfWork.DeleteWatchdogSearchCascade(_watchdogSearch);
    } 

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _watchdogSearch = new WatchdogSearchBuilder(newUnitOfWork).Build();
    }
}