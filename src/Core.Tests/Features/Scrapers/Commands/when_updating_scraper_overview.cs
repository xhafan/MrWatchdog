using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_updating_scraper_overview : BaseDatabaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new UpdateScraperOverviewCommandMessageHandler(new NhibernateRepository<Scraper>(UnitOfWork));

        await handler.Handle(new UpdateScraperOverviewCommand(new ScraperOverviewArgs
        {
            ScraperId = _scraper.Id,
            Name = "updated scraper name",
            Description = "updated scraper description",
            ScrapingIntervalInSeconds = 30,
            IntervalBetweenSameResultNotificationsInDays = 2.34,
            NumberOfFailedScrapingAttemptsBeforeAlerting = 2
        }));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _scraper = UnitOfWork.LoadById<Scraper>(_scraper.Id);
    }

    [Test]
    public void scraper_overview_is_updated()
    {
        _scraper.Name.ShouldBe("updated scraper name");
        _scraper.Description.ShouldBe("updated scraper description");
        _scraper.ScrapingIntervalInSeconds.ShouldBe(30);
        _scraper.IntervalBetweenSameResultNotificationsInDays.ShouldBe(2.34);
        _scraper.NumberOfFailedScrapingAttemptsBeforeAlerting.ShouldBe(2);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    }
}