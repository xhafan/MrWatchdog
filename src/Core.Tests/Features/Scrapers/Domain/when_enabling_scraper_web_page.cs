using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_enabling_scraper_web_page : BaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _scraper.EnableWebPage(_scraperWebPageId);
    }

    [Test]
    public void scraper_web_page_is_enabled()
    {
        var scraperWebPage = _scraper.WebPages.Single();
        scraperWebPage.IsEnabled.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingResults(_scraperWebPageId, ["Another World", "Doom 1"]);
    }
}