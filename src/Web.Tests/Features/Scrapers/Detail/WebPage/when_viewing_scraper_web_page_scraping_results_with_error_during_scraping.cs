using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_viewing_scraper_web_page_scraped_results_with_error_during_scraping : BaseDatabaseTest
{
    private WebPageScrapedResultsModel _model = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageScrapedResultsModelBuilder(UnitOfWork)
            .WithScraperId(_scraper.Id)
            .WithScraperWebPageId(_scraperWebPageId)
            .Build();

        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperWebPageScrapedResults.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperWebPageScrapedResults.ScraperWebPageId.ShouldBe(_scraperWebPageId);
        _model.ScraperWebPageScrapedResults.ScrapedResults.ShouldBeEmpty();
        _model.ScraperWebPageScrapedResults.ScrapedOn.ShouldBe(null);
        _model.ScraperWebPageScrapedResults.ScrapingErrorMessage.ShouldBe("Network error");
    }  

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingErrorMessage(_scraperWebPageId, "Network error");
    }    
}