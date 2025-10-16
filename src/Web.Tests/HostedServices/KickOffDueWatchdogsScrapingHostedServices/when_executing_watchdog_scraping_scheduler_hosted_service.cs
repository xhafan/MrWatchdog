using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.Web.HostedServices;

namespace MrWatchdog.Web.Tests.HostedServices.KickOffDueWatchdogsScrapingHostedServices;

[TestFixture]
public class when_executing_watchdog_scraping_scheduler_hosted_service : BaseDatabaseTest
{
    private Watchdog _watchdogWithoutNextScrapingOnSet = null!;
    private Watchdog _watchdogWithNextScrapingOnSetInTheFarPast = null!;
    private Watchdog _watchdogWithNextScrapingOnSetInTheFuture = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();
        
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Default", LogLevel.Debug)
                .AddConsole();
        });
        
        var logger = loggerFactory.CreateLogger<KickOffDueWatchdogsScrapingHostedService>();
        _bus = A.Fake<ICoreBus>();
        
        var options = A.Fake<IOptions<KickOffDueWatchdogsScrapingHostedServiceOptions>>();
        A.CallTo(() => options.Value).Returns(new KickOffDueWatchdogsScrapingHostedServiceOptions {IsDisabled = false});
        
        var hostedService = new KickOffDueWatchdogsScrapingHostedService(
            TestFixtureContext.NhibernateConfigurator,
            _bus,
            options,
            logger
        );
        hostedService.SetWatchdogIdsToScrape(
            _watchdogWithoutNextScrapingOnSet.Id,
            _watchdogWithNextScrapingOnSetInTheFarPast.Id,
            _watchdogWithNextScrapingOnSetInTheFuture.Id
        );

        using var cts = new CancellationTokenSource();

        await hostedService.StartAsync(cts.Token);

        await Task.Delay(200, cts.Token);
        await cts.CancelAsync();
        
        await hostedService.StopAsync(cts.Token);

        _watchdogWithoutNextScrapingOnSet = UnitOfWork.LoadById<Watchdog>(_watchdogWithoutNextScrapingOnSet.Id);
        _watchdogWithNextScrapingOnSetInTheFarPast = UnitOfWork.LoadById<Watchdog>(_watchdogWithNextScrapingOnSetInTheFarPast.Id);
        _watchdogWithNextScrapingOnSetInTheFuture = UnitOfWork.LoadById<Watchdog>(_watchdogWithNextScrapingOnSetInTheFuture.Id);
    }

    [Test]
    public void watchdog_without_next_scraping_on_timestamp_has_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeWatchdogCommand(_watchdogWithoutNextScrapingOnSet.Id))).MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void watchdog_with_next_scraping_on_timestamp_in_the_far_past_has_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeWatchdogCommand(_watchdogWithNextScrapingOnSetInTheFarPast.Id))).MustHaveHappenedOnceExactly();
    } 
    
    [Test]
    public void watchdog_with_next_scraping_on_timestamp_in_the_future_does_not_have_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeWatchdogCommand(_watchdogWithNextScrapingOnSetInTheFuture.Id))).MustNotHaveHappened();
    }
    
    [Test]
    public void watchdog_without_next_scraping_on_timestamp_has_next_scraping_on_set()
    {
        _watchdogWithoutNextScrapingOnSet.NextScrapingOn.ShouldNotBeNull();
        _watchdogWithoutNextScrapingOnSet.NextScrapingOn.Value.ShouldBe(DateTime.UtcNow.AddSeconds(60), tolerance: TimeSpan.FromSeconds(5));
    }
    
    [Test]
    public void watchdog_with_next_scraping_on_timestamp_in_the_far_past_has_next_scraping_on_set_to_the_future()
    {
        _watchdogWithNextScrapingOnSetInTheFarPast.NextScrapingOn!.Value.ShouldBe(DateTime.UtcNow.AddSeconds(120), tolerance: TimeSpan.FromSeconds(5));
    }     

    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteWatchdogCascade(_watchdogWithoutNextScrapingOnSet);
                await newUnitOfWork.DeleteWatchdogCascade(_watchdogWithNextScrapingOnSetInTheFarPast);
                await newUnitOfWork.DeleteWatchdogCascade(_watchdogWithNextScrapingOnSetInTheFuture);
            }
        );
    }    
    
    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _watchdogWithoutNextScrapingOnSet = new WatchdogBuilder(newUnitOfWork)
                    .WithScrapingIntervalInSeconds(60)
                    .Build();

                _watchdogWithNextScrapingOnSetInTheFarPast = new WatchdogBuilder(newUnitOfWork)
                    .WithScrapingIntervalInSeconds(120)
                    .WithNextScrapingOn(DateTime.UtcNow.AddYears(-1))
                    .Build();

                _watchdogWithNextScrapingOnSetInTheFuture = new WatchdogBuilder(newUnitOfWork)
                    .WithNextScrapingOn(DateTime.UtcNow.AddSeconds(120))
                    .Build();
            }
        );
    }
}