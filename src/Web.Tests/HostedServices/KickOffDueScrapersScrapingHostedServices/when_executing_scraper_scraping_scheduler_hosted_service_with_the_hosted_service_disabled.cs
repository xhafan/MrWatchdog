using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.HostedServices;

namespace MrWatchdog.Web.Tests.HostedServices.KickOffDueWatchdogsScrapingHostedServices;

[TestFixture]
public class when_executing_watchdog_scraping_scheduler_hosted_service_with_the_hosted_service_disabled : BaseDatabaseTest
{
    private Watchdog _watchdogWithoutNextScrapingOnSet = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();
        
        var logger = A.Fake<ILogger<KickOffDueWatchdogsScrapingHostedService>>();
        _bus = A.Fake<ICoreBus>();
        
        var options = A.Fake<IOptions<KickOffDueWatchdogsScrapingHostedServiceOptions>>();
        A.CallTo(() => options.Value).Returns(new KickOffDueWatchdogsScrapingHostedServiceOptions {IsDisabled = true});
        
        var hostedService = new KickOffDueWatchdogsScrapingHostedService(
            TestFixtureContext.NhibernateConfigurator,
            _bus,
            options,
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
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteWatchdogCascade(_watchdogWithoutNextScrapingOnSet);
            }
        );
    }    
    
    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _watchdogWithoutNextScrapingOnSet = new WatchdogBuilder(newUnitOfWork).Build();
            }
        );
    }
}