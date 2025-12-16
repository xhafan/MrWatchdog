using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Scraping;

[TestFixture]
public class when_scraping_scraper_without_any_web_page : BaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildScraper();

        var httpClientFactory = new HttpClientFactoryBuilder().Build();

        await _scraper.Scrape(new WebScraperChain([new HttpClientScraper(httpClientFactory)]));
    }

    [Test]
    public void scraper_scraping_completed_domain_event_is_not_raised()
    {
        RaisedDomainEvents.ShouldNotContain(new ScraperScrapingCompletedDomainEvent(_scraper.Id));
    }

    [Test]
    public void scraper_cannot_notify_about_failed_scraping()
    {
        _scraper.CanNotifyAboutFailedScraping.ShouldBe(false);
    }
    
    private void _BuildScraper()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage([])
            .Build();
    }
}