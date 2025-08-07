using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.HostedServices;

namespace MrWatchdog.Web.Tests.HostedServices.WatchdogScrapingSchedulerHostedServices;

[TestFixture]
public class when_executing_watchdog_scraping_scheduler_hosted_service_with_the_hosted_service_disabled : BaseDatabaseTest
{
    private Watchdog _watchdogWithoutNextScrapingOnSet = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();
        
        var logger = A.Fake<ILogger<WatchdogScrapingSchedulerHostedService>>();
        _bus = A.Fake<ICoreBus>();
        
        var watchdogScrapingSchedulerHostedServiceOptions = A.Fake<IOptions<WatchdogScrapingSchedulerHostedServiceOptions>>();
        A.CallTo(() => watchdogScrapingSchedulerHostedServiceOptions.Value).Returns(new WatchdogScrapingSchedulerHostedServiceOptions {IsDisabled = true});
        
        var hostedService = new WatchdogScrapingSchedulerHostedService(
            TestFixtureContext.NhibernateConfigurator,
            _bus,
            watchdogScrapingSchedulerHostedServiceOptions,
            logger
        );
        using var cts = new CancellationTokenSource();

        await hostedService.StartAsync(cts.Token);

        await Task.Delay(200, cts.Token);
        await cts.CancelAsync();
        
        await hostedService.StopAsync(cts.Token);
    }

    [Test]
    public void watchdog_without_next_scraping_on_timestamp_does_not_have_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeWatchdogCommand(_watchdogWithoutNextScrapingOnSet.Id))).MustNotHaveHappened();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        
        var watchdogRepository = new NhibernateRepository<Watchdog>(newUnitOfWork);
        var watchdog = await watchdogRepository.GetAsync(_watchdogWithoutNextScrapingOnSet.Id);
        if (watchdog != null)
        {
            await watchdogRepository.DeleteAsync(watchdog);
        }
    }    
    
    private void _BuildEntities()
    {
        _watchdogWithoutNextScrapingOnSet = new WatchdogBuilder(UnitOfWork).Build();
    }
}