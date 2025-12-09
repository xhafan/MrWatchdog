using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_viewing_scraper_web_page_scraping_results : BaseDatabaseTest
{
    private WebPageScrapingResultsModel _model = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageScrapingResultsModelBuilder(UnitOfWork)
            .WithScraperId(_scraper.Id)
            .WithScraperWebPageId(_scraperWebPageId)
            .Build();

        _actionResult = await _model.OnGet();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperWebPageScrapingResults.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperWebPageScrapingResults.ScraperWebPageId.ShouldBe(_scraperWebPageId);
        _model.ScraperWebPageScrapingResults.ScrapingResults.ShouldBe(["<div>text</div>"]);
        _model.ScraperWebPageScrapingResults.ScrapedOn.ShouldNotBeNull();
        _model.ScraperWebPageScrapingResults.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _model.ScraperWebPageScrapingResults.ScrapingErrorMessage.ShouldBe(null);
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
        _scraper.SetScrapingResults(_scraperWebPageId, ["<div>text</div>"]);
    }    
}