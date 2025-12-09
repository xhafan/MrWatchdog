using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_updating_existing_scraper_web_page_with_the_same_data_except_name : BaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _scraper.UpdateWebPage(new ScraperWebPageArgs
        {
            ScraperWebPageId = _scraperWebPageId,
            Url = "http://url.com/page",
            Selector = ".selector",
            SelectText = true,
            Name = "url.com/page2",
            HttpHeaders = """
                          User-Agent: Mozilla/5.0
                          Connection: keep-alive
                          """
        });
    }

    [Test]
    public void scraper_web_page_updated_domain_event_is_not_raised()
    {
        RaisedDomainEvents.ShouldNotContain(new ScraperWebPageScrapingDataUpdatedDomainEvent(_scraper.Id, _scraperWebPageId));
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = true,
                Name = "url.com/page",
                HttpHeaders = """
                              Connection: keep-alive
                              User-Agent: Mozilla/5.0
                              """
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }
}