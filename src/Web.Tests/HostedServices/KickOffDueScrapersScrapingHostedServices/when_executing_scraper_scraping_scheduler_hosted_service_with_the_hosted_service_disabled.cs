using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.HostedServices;

namespace MrWatchdog.Web.Tests.HostedServices.KickOffDueScrapersScrapingHostedServices;

[TestFixture]
public class when_executing_scraper_scraping_scheduler_hosted_service_with_the_hosted_service_disabled : BaseDatabaseTest
{
    private Scraper _scraperWithoutNextScrapingOnSet = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();
        
        var logger = A.Fake<ILogger<KickOffDueScrapersScrapingHostedService>>();
        _bus = A.Fake<ICoreBus>();
        
        var options = A.Fake<IOptions<KickOffDueScrapersScrapingHostedServiceOptions>>();
        A.CallTo(() => options.Value).Returns(new KickOffDueScrapersScrapingHostedServiceOptions {IsDisabled = true});
        
        var hostedService = new KickOffDueScrapersScrapingHostedService(
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
    public void scraper_without_next_scraping_on_timestamp_does_not_have_scraping_scheduled()
    {
        A.CallTo(() => _bus.Send(new ScrapeScraperCommand(_scraperWithoutNextScrapingOnSet.Id))).MustNotHaveHappened();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteScraperCascade(_scraperWithoutNextScrapingOnSet);
            }
        );
    }    
    
    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _scraperWithoutNextScrapingOnSet = new ScraperBuilder(newUnitOfWork).Build();
            }
        );
    }
}