using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_getting_watchdog_detail_page : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
        
        UnitOfWork.Commit();
        UnitOfWork.BeginTransaction();        
    }

    [Test]
    public async Task watchdog_detail_page_can_be_fetched()
    {
        var watchdogDetailUrl = WatchdogWebConstants.WatchdogDetailUrl.Replace(WatchdogWebConstants.WatchdogIdVariable, $"{_watchdog.Id}");
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(watchdogDetailUrl);
        response.EnsureSuccessStatusCode();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        var watchdogRepository = new NhibernateRepository<Watchdog>(UnitOfWork);
        var watchdog = await watchdogRepository.GetAsync(_watchdog.Id);
        if (watchdog != null)
        {
            await watchdogRepository.DeleteAsync(watchdog);
        }
        
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();         
    }    
}