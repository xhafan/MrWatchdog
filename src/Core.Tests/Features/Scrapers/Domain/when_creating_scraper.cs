using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_creating_scraper : BaseDatabaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public void Context()
    {
        var user = new UserBuilder().Build();
        _scraper = new Scraper(user, "scraper name");
    }

    [Test]
    public void default_values_are_set_correctly()
    {
        _scraper.ScrapingIntervalInSeconds.ShouldBe(Scraper.DefaultScrapingIntervalOneDayInSeconds);
        _scraper.IntervalBetweenSameResultNotificationsInDays.ShouldBe(Scraper.DefaultIntervalBetweenSameResultNotificationsInDays);
    }
}