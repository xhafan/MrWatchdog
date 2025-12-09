using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_persisting_scraper : BaseDatabaseTest
{
    private Scraper _newScraper = null!;
    private Scraper? _persistedScraper;

    [SetUp]
    public void Context()
    {
        _newScraper = new ScraperBuilder(UnitOfWork).Build();
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedScraper = UnitOfWork.Get<Scraper>(_newScraper.Id);
    }

    [Test]
    public void persisted_scraper_can_be_retrieved_and_has_correct_data()
    {
        _persistedScraper.ShouldNotBeNull();
        _persistedScraper.ShouldBe(_newScraper);

        _persistedScraper.User.ShouldBe(_newScraper.User);
        _persistedScraper.Name.ShouldBe(ScraperBuilder.Name);
        _persistedScraper.Description.ShouldBe(ScraperBuilder.Description);
        _persistedScraper.ScrapingIntervalInSeconds.ShouldBe(ScraperBuilder.ScrapingIntervalInSeconds);
        _persistedScraper.PublicStatus.ShouldBe(PublicStatus.Private);
        _persistedScraper.IntervalBetweenSameResultNotificationsInDays.ShouldBe(ScraperBuilder.IntervalBetweenSameResultNotificationsInDays);
        _persistedScraper.CanNotifyAboutFailedScraping.ShouldBe(false);
        _persistedScraper.NumberOfFailedScrapingAttemptsBeforeAlerting.ShouldBe(5);
        _persistedScraper.IsArchived.ShouldBe(false);
    }
}