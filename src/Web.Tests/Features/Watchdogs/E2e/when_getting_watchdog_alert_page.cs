using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdog_alert_page : BaseDatabaseTest
{
    private WatchdogAlert _watchdogAlert = null!;

    [SetUp]
    public void Context()
    {
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork).Build();
        
        UnitOfWork.Commit();
        UnitOfWork.BeginTransaction();
    }

    [Test]
    public async Task watchdog_alert_page_can_be_fetched()
    {
        var watchdogAlertUrl = WatchdogConstants.WatchdogAlertUrl
            .Replace(WatchdogConstants.WatchdogAlertIdVariable, $"{_watchdogAlert.Id}");
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(watchdogAlertUrl);
        response.EnsureSuccessStatusCode();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        var watchdogAlertRepository = new NhibernateRepository<WatchdogAlert>(UnitOfWork);
        var watchdogAlert = await watchdogAlertRepository.GetAsync(_watchdogAlert.Id);
        if (watchdogAlert != null)
        {
            await watchdogAlertRepository.DeleteAsync(watchdogAlert);

            var watchdogRepository = new NhibernateRepository<Watchdog>(UnitOfWork);
            var watchdog = await watchdogRepository.LoadByIdAsync(_watchdogAlert.Watchdog.Id);
            await watchdogRepository.DeleteAsync(watchdog);
        }
        
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();         
    }    
}