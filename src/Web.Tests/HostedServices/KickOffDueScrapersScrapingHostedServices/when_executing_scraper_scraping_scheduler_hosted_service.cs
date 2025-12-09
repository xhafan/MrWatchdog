using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.Web.HostedServices;

namespace MrWatchdog.Web.Tests.HostedServices.KickOffDueScrapersScrapingHostedServices;

[TestFixture]
public class when_executing_scraper_scraping_scheduler_hosted_service : BaseDatabaseTest
{
    private Scraper _scraperWithoutNextScrapingOnSet = null!;
    private Scraper _scraperWithNextScrapingOnSetInTheFarPast = null!;
    private Scraper _scraperWithNextScrapingOnSetInTheFuture = null!;
    private Scraper _archivedScraperWithoutNextScrapingOnSet = null!;
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
        
        var logger = loggerFactory.CreateLogger<KickOffDueScrapersScrapingHostedService>();
        _bus = A.Fake<ICoreBus>();
        
        var options = A.Fake<IOptions<KickOffDueScrapersScrapingHostedServiceOptions>>();
        A.CallTo(() => options.Value).Returns(new KickOffDueScrapersScrapingHostedServiceOptions {IsDisabled = false});
        
        var hostedService = new KickOffDueScrapersScrapingHostedService(
            TestFixtureContext.NhibernateConfigurator,
            _bus,
            options,
            logger
        );
        hostedService.SetScraperIdsToScrape(
            _scraperWithoutNextScrapingOnSet.Id,
            _scraperWithNextScrapingOnSetInTheFarPast.Id,
            _scraperWithNextScrapingOnSetInTheFuture.Id,
            _archivedScraperWithoutNextScrapingOnSet.Id
        );

        using var cts = new CancellationTokenSource();

        await hostedService.StartAsync(cts.Token);

        await Task.Delay(200, cts.Token);
        await cts.CancelAsync();
        
        await hostedService.StopAsync(cts.Token);

        _scraperWithoutNextScrapingOnSet = UnitOfWork.LoadById<Scraper>(_scraperWithoutNextScrapingOnSet.Id);
        _scraperWithNextScrapingOnSetInTheFarPast = UnitOfWork.LoadById<Scraper>(_scraperWithNextScrapingOnSetInTheFarPast.Id);
        _scraperWithNextScrapingOnSetInTheFuture = UnitOfWork.LoadById<Scraper>(_scraperWithNextScrapingOnSetInTheFuture.Id);
        _archivedScraperWithoutNextScrapingOnSet = UnitOfWork.LoadById<Scraper>(_archivedScraperWithoutNextScrapingOnSet.Id);
    }

    [Test]
    public void scraper_without_next_scraping_on_timestamp_has_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeScraperCommand(_scraperWithoutNextScrapingOnSet.Id))).MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void scraper_with_next_scraping_on_timestamp_in_the_far_past_has_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeScraperCommand(_scraperWithNextScrapingOnSetInTheFarPast.Id))).MustHaveHappenedOnceExactly();
    } 
    
    [Test]
    public void scraper_with_next_scraping_on_timestamp_in_the_future_does_not_have_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeScraperCommand(_scraperWithNextScrapingOnSetInTheFuture.Id))).MustNotHaveHappened();
    }
    
    [Test]
    public void archived_scraper_does_not_have_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeScraperCommand(_archivedScraperWithoutNextScrapingOnSet.Id))).MustNotHaveHappened();
    }

    [Test]
    public void scraper_without_next_scraping_on_timestamp_has_next_scraping_on_set()
    {
        _scraperWithoutNextScrapingOnSet.NextScrapingOn.ShouldNotBeNull();
        _scraperWithoutNextScrapingOnSet.NextScrapingOn.Value.ShouldBe(DateTime.UtcNow.AddSeconds(60), tolerance: TimeSpan.FromSeconds(5));
    }
    
    [Test]
    public void scraper_with_next_scraping_on_timestamp_in_the_far_past_has_next_scraping_on_set_to_the_future()
    {
        _scraperWithNextScrapingOnSetInTheFarPast.NextScrapingOn!.Value.ShouldBe(DateTime.UtcNow.AddSeconds(120), tolerance: TimeSpan.FromSeconds(5));
    }     

    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteScraperCascade(_scraperWithoutNextScrapingOnSet);
                await newUnitOfWork.DeleteScraperCascade(_scraperWithNextScrapingOnSetInTheFarPast);
                await newUnitOfWork.DeleteScraperCascade(_scraperWithNextScrapingOnSetInTheFuture);
                await newUnitOfWork.DeleteScraperCascade(_archivedScraperWithoutNextScrapingOnSet);
            }
        );
    }    
    
    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _scraperWithoutNextScrapingOnSet = new ScraperBuilder(newUnitOfWork)
                    .WithScrapingIntervalInSeconds(60)
                    .Build();

                _scraperWithNextScrapingOnSetInTheFarPast = new ScraperBuilder(newUnitOfWork)
                    .WithScrapingIntervalInSeconds(120)
                    .WithNextScrapingOn(DateTime.UtcNow.AddYears(-1))
                    .Build();

                _scraperWithNextScrapingOnSetInTheFuture = new ScraperBuilder(newUnitOfWork)
                    .WithNextScrapingOn(DateTime.UtcNow.AddSeconds(120))
                    .Build();

                _archivedScraperWithoutNextScrapingOnSet = new ScraperBuilder(newUnitOfWork)
                    .WithScrapingIntervalInSeconds(60)
                    .Build();
                _archivedScraperWithoutNextScrapingOnSet.Archive();
            }
        );
    }
}