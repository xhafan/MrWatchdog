using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_updating_scraper_overview
{
    private Scraper _scraper = null!;
    private readonly DateTime _nextScrapingOn = DateTime.UtcNow;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _scraper.UpdateOverview(new ScraperOverviewArgs
        {
            ScraperId = _scraper.Id,
            Name = "scraper name",
            Description = null,
            ScrapingIntervalInSeconds = 30,
            IntervalBetweenSameResultNotificationsInDays = 2.34,
            NumberOfFailedScrapingAttemptsBeforeAlerting = 5
        });
    }

    [Test]
    public void scraper_next_scraping_on_is_adjusted()
    {
        _scraper.NextScrapingOn.ShouldBe(_nextScrapingOn.AddSeconds(-60 + 30));
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithScrapingIntervalInSeconds(60)
            .WithNextScrapingOn(_nextScrapingOn)
            .Build();
    }
}